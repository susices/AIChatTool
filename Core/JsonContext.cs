using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIChatTool;

[JsonSerializable(typeof(ChatRequest))]
[JsonSerializable(typeof(ChatCompletion))]
[JsonSerializable(typeof(ChatCompletionChunk))]
[JsonSerializable(typeof(Dictionary<string,string>))]
public partial class JsonContext : JsonSerializerContext
{
    public static JsonContext Context { get; } = new(new JsonSerializerOptions()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    });
}