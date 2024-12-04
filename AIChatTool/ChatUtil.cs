using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AIChatTool.FunctionCalls;
using Cysharp.Threading.Tasks;

namespace AIChatTool;

public static class ChatUtil
{
    private static readonly HttpClient httpClient = new()
    {
        BaseAddress = new Uri("https://api.deepseek.com"),
        Timeout = Timeout.InfiniteTimeSpan
    };

    public static async UniTask<ChatCompletion> SendChatMsg(ChatRequest chatRequest)
    {
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigUtil.API_KEY);

        var jsonContent = JsonSerializer.Serialize(chatRequest, JsonContext.Default.ChatRequest);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = content
        };

        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
        {
            // Handle error response
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
        }

        if (chatRequest.Stream)
        {
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var streamReader = new StreamReader(stream, Encoding.UTF8))
            {
                var responseContentBuilder = new StringBuilder();
                string? line;

                while ((line = await streamReader.ReadLineAsync()) != null)
                {
                    if (line.StartsWith("data: "))
                    {
                        var jsonChunk = line.Substring("data: ".Length);

                        if (jsonChunk == "[DONE]") break;

                        var chunk = JsonSerializer.Deserialize<ChatCompletionChunk>(jsonChunk,
                            JsonContext.Default.ChatCompletionChunk);

                        if (chunk.Choices != null && chunk.Choices.Count > 0)
                        {
                            var deltaContent = chunk.Choices[0].Delta?.Content;
                            if (!string.IsNullOrEmpty(deltaContent))
                            {
                                responseContentBuilder.Append(deltaContent);
                                Console.Write(deltaContent);
                            }
                        }
                    }
                }
                    

                Console.Write("\n");

                // 返回最终的 ChatCompletion 对象
                var responseContent = responseContentBuilder.ToString();
                var finalChatCompletion = new ChatCompletion
                {
                    Choices = new List<Choice>
                    {
                        new()
                        {
                            Message = new Message
                            {
                                Role = "assistant",
                                Content = responseContent
                            }
                        }
                    }
                };
                return finalChatCompletion;
            }
        }
        else
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            //Console.WriteLine($"Debug: {responseContent}");
            var chatCompletion = JsonSerializer.Deserialize<ChatCompletion>(responseContent, JsonContext.Default.ChatCompletion);
            if (chatCompletion==null)
            {
                Console.WriteLine("json parse ChatCompletion error");
                return null;
            }
            var message = chatCompletion?.Choices?.FirstOrDefault()?.Message;
            if (message==null)
            {
                return null;
            }

            if (message.ToolCalls?.Count>0)
            {
                List<Message>? toolCallResults = await HandleToolCalls(chatRequest,message.ToolCalls);
                if (toolCallResults==null)
                {
                    Console.WriteLine("tool call error");
                    return null;
                }
                chatRequest.Messages.Add(message);
                chatRequest.Messages.AddRange(toolCallResults);
                return await SendChatMsg(chatRequest);
            }
            else
            {
                Console.WriteLine(chatCompletion?.Choices?.FirstOrDefault()?.Message.Content);
            }
            
            return chatCompletion;
        }
        
    }
    
    public static async UniTask<List<Message>> HandleToolCalls(ChatRequest chatRequest,List<ToolCall> toolCalls)
    {
        var tasks = new List<UniTask<Message>>();
        foreach (var toolCall in toolCalls)
        {
            tasks.Add(HandleToolCall(chatRequest, toolCall));
        }
        var result = await UniTask.WhenAll(tasks);
        return result.ToList();
    }

    public static async UniTask<Message> HandleToolCall(ChatRequest chatRequest, ToolCall toolCall)
    {
        await UniTask.CompletedTask;
        var result = await FunctionCallDispatcher.Dispatch(toolCall.Function.Name, toolCall.Function.Arguments);
        
        return new Message()
        {
            Role = "tool",
            ToolCallId = toolCall.Id,
            Content = result,
        };
    }
}