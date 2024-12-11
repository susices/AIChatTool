using System.Text.Json;
using AIChatTool;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/chat/completions", async (HttpContext context, [FromBody] ChatRequest request) =>
{
    var chatClient = new ChatClient(ModelConfig.QwenPlus, request);
    var response = await chatClient.SendChatMsg();
    
    var chunk = new ChatCompletionChunk
    {
        Id = response.Id,
        Object = "chat.completion.chunk",
        Created = response.Created,
        Model = response.Model, // 或你的模型
        SystemFingerprint = null,
        Choices = new List<ChoiceChunk>
        {
            new ChoiceChunk
            {
                Index = 0,
                Delta = new Delta { Content = response.Choices[0].Message.Content},
                FinishReason = null
            }
        }
    };
    var json = JsonSerializer.Serialize(chunk, JsonContext.Context.ChatCompletionChunk);
    // 构建 SSE 消息格式
    var message = $"data: {json}\n\n";
    await context.Response.WriteAsync(message);
    await context.Response.Body.FlushAsync(); // 确保消息立即发送
    
    // 发送结束消息
    await context.Response.WriteAsync("data: [DONE]\n\n");
    await context.Response.Body.FlushAsync();
    
});

// app.MapPost("/chat/completions", async ([FromBody] ChatRequest request) =>
// {
//     return Results.Ok(request);
// });

app.Run();
