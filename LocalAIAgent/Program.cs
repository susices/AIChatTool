using System.Text.Json;
using AIChatTool;
using Cysharp.Threading.Tasks;
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

app.MapPost("/chat/completions", HandleChatCompletion);

// app.MapPost("/chat/completions", async ([FromBody] ChatRequest request) =>
// {
//     return Results.Ok(request);
// });

app.Run();


async Task HandleChatCompletion(HttpContext context, [FromBody] ChatRequest request)
{
    bool isStreamRequest = request.Stream;
    string modelurl = context.Request.Query["modelurl"];
    if (modelurl==null)
    {
        Console.WriteLine(" modelurl is null");
        return;
    }
    // 获取context中的 apikey
    var apiKey = context.Request.Headers["Authorization"].ToString().Split(" ")[1];
    // 强制关闭stream
    request.Stream = false;
    var chatClient = new ChatClient(new ModelInfo()
    {
        ModelName = request.Model,
        Url = modelurl
    },apiKey, request);
    ChatCompletion? response;
    try
    {
        response = await chatClient.SendChatMsg();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return;
    }

    if (response==null)
    {
        Console.WriteLine("response is null");
        return;
    }
    
    // 非流式返回
    if (!isStreamRequest)
    {
        var json = JsonSerializer.Serialize(response, JsonContext.Context.ChatCompletion);
        await context.Response.WriteAsync(json);
        await context.Response.Body.FlushAsync();
    }
    // 流式返回
    else
    {
        var chunk = new ChatCompletionChunk
        {
            Id = response.Id,
            Object = "chat.completion.chunk",
            Created = response.Created,
            Model = response.Model, 
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
    }
} 
