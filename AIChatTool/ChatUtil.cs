using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace AIChatTool
{
    public static class ChatUtil
    {
        private static readonly HttpClient httpClient = new HttpClient
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

    if (response.IsSuccessStatusCode)
    {
        using (var stream = await response.Content.ReadAsStreamAsync())
        using (var streamReader = new StreamReader(stream, Encoding.UTF8))
        {
            StringBuilder responseContentBuilder = new StringBuilder();
            string line;

            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("data: "))
                {
                    var jsonChunk = line.Substring("data: ".Length);

                    if (jsonChunk == "[DONE]")
                    {
                        break;
                    }

                    var chunk = JsonSerializer.Deserialize<ChatCompletionChunk>(jsonChunk, JsonContext.Default.ChatCompletionChunk);

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
                    new Choice
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
        // Handle error response
        var errorContent = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
    }
}
        
    }
}