using System.Text.Json;
using System.Text.Json.Serialization;
using Cysharp.Threading.Tasks;
using OpenAI.Chat;
using RestSharp;

namespace AIChatTool;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        await TestChat();
        //TestJson();
    }

    public static void TestJson()
    {
        string json = """
                      {
                        "id": "cc8227d7-9802-4a67-bddc-63c75ff5e57a",
                        "object": "chat.completion",
                        "created": 1732636103,
                        "model": "deepseek-chat",
                        "choices": [
                          {
                            "index": 0,
                            "message": {
                              "role": "assistant",
                              "content": "Hello! How can I assist you today?"
                            },
                            "logprobs": null,
                            "finish_reason": "stop"
                          }
                        ],
                        "usage": {
                          "prompt_tokens": 9,
                          "completion_tokens": 9,
                          "total_tokens": 18,
                          "prompt_cache_hit_tokens": 0,
                          "prompt_cache_miss_tokens": 9
                        },
                        "system_fingerprint": "fp_1c141eb703"
                      }
                      """;
        try
        {
            ChatCompletion chatCompletion = JsonSerializer.Deserialize<ChatCompletion>(json);
            Console.WriteLine("Deserialization successful!");
            Console.WriteLine($"Id: {chatCompletion.Id}");
            Console.WriteLine($"Object: {chatCompletion.Object}");
            Console.WriteLine($"Created: {chatCompletion.Created}");
            Console.WriteLine($"Model: {chatCompletion.Model}");
            Console.WriteLine($"SystemFingerprint: {chatCompletion.SystemFingerprint}");

            if (chatCompletion.Choices != null && chatCompletion.Choices.Count > 0)
            {
                Choice firstChoice = chatCompletion.Choices[0];
                Console.WriteLine($"Choice Index: {firstChoice.Index}");
                Console.WriteLine($"Choice Message Role: {firstChoice.Message.Role}");
                Console.WriteLine($"Choice Message Content: {firstChoice.Message.Content}");
                Console.WriteLine($"Choice Finish Reason: {firstChoice.FinishReason}");
            }

            if (chatCompletion.Usage != null)
            {
                Console.WriteLine($"Usage Prompt Tokens: {chatCompletion.Usage.PromptTokens}");
                Console.WriteLine($"Usage Completion Tokens: {chatCompletion.Usage.CompletionTokens}");
                Console.WriteLine($"Usage Total Tokens: {chatCompletion.Usage.TotalTokens}");
                Console.WriteLine($"Usage Prompt Cache Hit Tokens: {chatCompletion.Usage.PromptCacheHitTokens}");
                Console.WriteLine($"Usage Prompt Cache Miss Tokens: {chatCompletion.Usage.PromptCacheMissTokens}");
            }
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"ArgumentNullException: {ex.Message}");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JsonException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General Exception: {ex.Message}");
        }
    }
    
    public static async UniTask TestChat()
    {
        var options = new RestClientOptions("https://api.deepseek.com")
        {
            MaxTimeout = -1
        };
        var client = new RestClient(options);
        var request = new RestRequest("https://api.deepseek.com/chat/completions", Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Authorization", "Bearer sk-d484a5253df44cba8435ffdfab8430d3");
        var body = """
                   {
                     "messages": [
                       {
                         "content": "You are a helpful assistant",
                         "role": "system"
                       },
                       {
                         "content": "Hi",
                         "role": "user"
                       }
                     ],
                     "model": "deepseek-chat",
                     "frequency_penalty": 0,
                     "max_tokens": 2048,
                     "presence_penalty": 0,
                     "response_format": {
                       "type": "text"
                     },
                     "stop": null,
                     "stream": false,
                     "stream_options": null,
                     "temperature": 1,
                     "top_p": 1,
                     "tools": null,
                     "tool_choice": "none",
                     "logprobs": false,
                     "top_logprobs": null
                   }
                   """;
        request.AddStringBody(body, DataFormat.Json);
        var response = await client.ExecuteAsync(request);
        // 将response content 转换为chatcompletion
        //Console.WriteLine(response.Content);
        var chatCompletion = JsonSerializer.Deserialize<ChatCompletion>(response.Content);
        Console.WriteLine(chatCompletion?.Choices?.Last()?.Message?.Content);
    }

    public class ChatCompletionResponse
    {
        public string id { get; set; }
        
    }
}