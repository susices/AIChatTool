using System.Text.Json.Serialization;

namespace AIChatTool;

[JsonSerializable(typeof(ChatRequest))]
[JsonSerializable(typeof(ChatCompletion))]
[JsonSerializable(typeof(ChatCompletionChunk))]
internal partial class JsonContext : JsonSerializerContext
{
}
