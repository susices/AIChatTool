using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AIChatTool.FunctionCalls;
using Cysharp.Threading.Tasks;

namespace AIChatTool;

public static class ChatUtil
{
    public static async UniTask<ChatCompletion?> SendChatMsg(this ChatClient chatClient)
{
    try
    {
        var chatRequest = chatClient.ChatRequest;
        var httpClient = chatClient.httpClient;

        var jsonContent = JsonSerializer.Serialize(chatRequest, JsonContext.Context.ChatRequest);
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
            throw new HttpRequestException(
                $"Request failed with status code {response.StatusCode}: {errorContent}");
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
                        
                        //Console.WriteLine(line);

                        if (chunk?.Choices is { Count: > 0 })
                        {
                            var deltaContent = chunk.Choices[0].Delta?.Content;
                            if (!string.IsNullOrEmpty(deltaContent))
                            {
                                responseContentBuilder.Append(deltaContent);
                                Console.Write(deltaContent);
                            }
                        
                            // Handle tool calls in streaming mode
                            var toolCalls = chunk.Choices[0].Delta?.ToolCalls;
                            if (toolCalls is { Count: > 0 })
                            {
                                ToolCall? toolCall = toolCalls[0];
                                toolCall = await ParseToolCallOnStream(toolCall, streamReader);
                                if (toolCall==null)
                                {
                                    Console.WriteLine("tool call error");
                                    return null;
                                }
                                Console.WriteLine($"toolCall args: {toolCall.Function.Arguments}");
                                Message? toolCallResult = await HandleToolCall(chatRequest, toolCall);
                                if (toolCallResult == null)
                                {
                                    Console.WriteLine("tool call error");
                                    return null;
                                }
                                chatRequest.Messages.Add(new Message()
                                {
                                    Role = "assistant",
                                    ToolCallId = toolCall.Id,
                                    ToolCalls = new List<ToolCall> { toolCall },
                                });
                                chatRequest.Messages.Add(toolCallResult);
                                return await chatClient.SendChatMsg();
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
            var chatCompletion =
                JsonSerializer.Deserialize<ChatCompletion>(responseContent, JsonContext.Default.ChatCompletion);
            if (chatCompletion == null)
            {
                Console.WriteLine("json parse ChatCompletion error");
                return null;
            }

            var message = chatCompletion?.Choices?.FirstOrDefault()?.Message;
            if (message == null) return null;

            if (message.ToolCalls?.Count > 0)
            {
                List<Message>? toolCallResults = await HandleToolCalls(chatRequest, message.ToolCalls);
                if (toolCallResults == null)
                {
                    Console.WriteLine("tool call error");
                    return null;
                }

                chatRequest.Messages.Add(message);
                chatRequest.Messages.AddRange(toolCallResults);
                return await chatClient.SendChatMsg();
            }

            Console.WriteLine(chatCompletion?.Choices?.FirstOrDefault()?.Message.Content);

            return chatCompletion;
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return null;
    }
}

    public static async UniTask<List<Message>> HandleToolCalls(ChatRequest chatRequest, List<ToolCall> toolCalls)
    {
        var tasks = new List<UniTask<Message>>();
        foreach (var toolCall in toolCalls) tasks.Add(HandleToolCall(chatRequest, toolCall));
        var result = await UniTask.WhenAll(tasks);
        return result.ToList();
    }

    public static async UniTask<Message> HandleToolCall(ChatRequest chatRequest, ToolCall toolCall)
    {
        await UniTask.CompletedTask;
        var result = await FunctionCallDispatcher.Dispatch(toolCall.Function.Name, toolCall.Function.Arguments);

        return new Message
        {
            Role = "tool",
            ToolCallId = toolCall.Id,
            Content = result
        };
    }

    private static async UniTask<ToolCall?> ParseToolCallOnStream(ToolCall toolCallFirstChunk,StreamReader streamReader)
    {
        string? line;
    
        while ((line = await streamReader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }
            
            if (!line.StartsWith("data: "))
            {
                Console.WriteLine($"ParseToolCallOnStream error line:{line}");
                return null;
            }
            
            var jsonChunk = line.Substring("data: ".Length);
            if (jsonChunk == "[DONE]") break;
            var chunk = JsonSerializer.Deserialize<ChatCompletionChunk>(jsonChunk,
                JsonContext.Default.ChatCompletionChunk);
            if (chunk?.Choices?.FirstOrDefault()?.FinishReason!=null)
            {
                break;
            }
            var argsDelta = chunk?.Choices?.FirstOrDefault()?.Delta?.ToolCalls.FirstOrDefault()?.Function.Arguments;
            if (argsDelta==null)
            {
                break;
            }
            toolCallFirstChunk.Function.Arguments += argsDelta;
        }
        
        return toolCallFirstChunk;
    }
}