using System.Text.Json.Serialization;

namespace AIChatTool;

public class ChatCompletion
{
    [JsonPropertyName("id")] public string? Id { get; set; }

    [JsonPropertyName("object")] public string? Object { get; set; }

    [JsonPropertyName("created")] public long Created { get; set; }

    [JsonPropertyName("model")] public string? Model { get; set; }

    [JsonPropertyName("choices")] public List<Choice>? Choices { get; set; }

    [JsonPropertyName("usage")] public Usage? Usage { get; set; }

    [JsonPropertyName("system_fingerprint")]
    public string? SystemFingerprint { get; set; }
}

public class Choice
{
    [JsonPropertyName("index")] public int Index { get; set; }

    [JsonPropertyName("message")] public Message Message { get; set; }

    [JsonPropertyName("logprobs")] public object Logprobs { get; set; }

    [JsonPropertyName("finish_reason")] public string FinishReason { get; set; }
}

public class Message
{
    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("tool_calls")]
    public List<ToolCall> ToolCalls { get; set; }
    
    [JsonPropertyName("tool_call_id")]
    public string ToolCallId { get; set; }
}

public class ToolCall
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("function")]
    public FunctionCall Function { get; set; }
}

public class FunctionCall
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("arguments")]
    public string Arguments { get; set; }
}

public class Usage
{
    [JsonPropertyName("prompt_tokens")] public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")] public int TotalTokens { get; set; }

    [JsonPropertyName("prompt_cache_hit_tokens")]
    public int PromptCacheHitTokens { get; set; }

    [JsonPropertyName("prompt_cache_miss_tokens")]
    public int PromptCacheMissTokens { get; set; }
}

public class ChatCompletionChunk
{
    [JsonPropertyName("id")] public string? Id { get; set; }

    [JsonPropertyName("object")] public string? Object { get; set; }

    [JsonPropertyName("created")] public long Created { get; set; }

    [JsonPropertyName("model")] public string? Model { get; set; }

    [JsonPropertyName("choices")] public List<ChoiceChunk>? Choices { get; set; }

    [JsonPropertyName("system_fingerprint")]
    public string? SystemFingerprint { get; set; }
}

public class ChoiceChunk
{
    [JsonPropertyName("index")] public int Index { get; set; }

    [JsonPropertyName("delta")] public Delta Delta { get; set; }

    [JsonPropertyName("logprobs")] public object Logprobs { get; set; }

    [JsonPropertyName("finish_reason")] public string FinishReason { get; set; }
}

public class Delta
{
    [JsonPropertyName("role")] public string Role { get; set; }

    [JsonPropertyName("content")] public string Content { get; set; }
}