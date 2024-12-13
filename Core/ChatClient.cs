using System.Net.Security;
using AIChatTool.FunctionCalls;
using Cysharp.Threading.Tasks;

namespace AIChatTool;

public class ChatClient
{
    public ChatRequest ChatRequest;

    public HttpClient httpClient;
    
    public readonly ModelInfo ModelInfo;

    public ChatClient(ModelInfo modelInfo, ChatRequest? chatRequest=null)
    {
        ModelInfo = modelInfo;
        if (chatRequest==null)
        {
            chatRequest = new ChatRequest();
            chatRequest.Messages = new List<Message>
            {
                new()
                {
                    Role = "system",
                    Content = "You are a helpful assistant."
                }
            };
            //ChatRequest.Model = "deepseek-chat";
            chatRequest.Model = modelInfo.ModelName;
            chatRequest.FrequencyPenalty = 0;
            chatRequest.MaxTokens = 2048;
            chatRequest.PresencePenalty = 0;
            chatRequest.ResponseFormat = new ResponseFormat
            {
                Type = "text"
            };
            chatRequest.Stop = null;
            chatRequest.Stream = false;
            chatRequest.StreamOptions = null;
            chatRequest.Temperature = 1;
            chatRequest.TopP = 1;
            chatRequest.Tools = null;
            chatRequest.ToolChoice = "auto";
            chatRequest.Logprobs = false;
            chatRequest.TopLogprobs = null;
        }
        ChatRequest = chatRequest;
        
        var tools = new List<Tool>();
        ChatRequest.Tools = tools;
        tools.Add(new Tool
        {
            Type = "function",
            Function = FunctionCallGetWeather.Function
        });
        ModelInfo = modelInfo;
        HttpClientHandler handler = new();
        handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
        {
            // 检查主机名是否为 localhost 或 127.0.0.1
            if (sender is HttpRequestMessage request && 
                (request.RequestUri.Host == "localhost" || request.RequestUri.Host == "127.0.0.1")) 
            {
                return true; // 对 localhost 地址始终返回 true，忽略证书错误
            }

            // 对于非 localhost 地址，执行默认的证书验证逻辑 (生产环境中必须这样做)
            return sslPolicyErrors == SslPolicyErrors.None;
        };
        httpClient =  new(handler)
        {
            BaseAddress = new Uri(modelInfo.Url),
            Timeout = Timeout.InfiniteTimeSpan,
        };
    }

    public async UniTask<string?> SendChatMsg(string msg)
    {
        ChatRequest.Messages.Add(new Message
        {
            Role = "user",
            Content = msg
        });

        var chatCompletion = await this.SendChatMsg();
        if (chatCompletion==null)
        {
            return null;
        }
        var result = chatCompletion.Choices[0].Message;
        ChatRequest.Messages.Add(result);
        return result.Content;
    }
}