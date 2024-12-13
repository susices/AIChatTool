using System.Text.Json.Serialization;

namespace AIChatTool;

public class ChatRequest
{
    [JsonPropertyName("messages")] public List<Message> Messages { get; set; }

    [JsonPropertyName("model")] public string Model { get; set; }

    [JsonPropertyName("frequency_penalty")]
    public int? FrequencyPenalty { get; set; }

    [JsonPropertyName("max_tokens")] public int? MaxTokens { get; set; }

    [JsonPropertyName("presence_penalty")] public int? PresencePenalty { get; set; }

    [JsonPropertyName("response_format")] public ResponseFormat ResponseFormat { get; set; }

    [JsonPropertyName("stop")] public object? Stop { get; set; }

    [JsonPropertyName("stream")] public bool Stream { get; set; }

    
    [JsonPropertyName("stream_options")] public object? StreamOptions { get; set; }

    [JsonPropertyName("temperature")] public float Temperature { get; set; }

    [JsonPropertyName("top_p")] public float TopP { get; set; }

    [JsonPropertyName("tools")]  public List<Tool> Tools { get; set; }

    [JsonPropertyName("tool_choice")] public string ToolChoice { get; set; }

    [JsonPropertyName("logprobs")] public bool? Logprobs { get; set; }

    [JsonPropertyName("top_logprobs")] public object? TopLogprobs { get; set; }
}

public class ResponseFormat
{
    [JsonPropertyName("type")] public string Type { get; set; }
}


