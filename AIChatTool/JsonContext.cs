using System.Text.Json.Serialization;

namespace AIChatTool;

[JsonSerializable(typeof(ChatRequest))]
[JsonSerializable(typeof(ChatCompletion))]
[JsonSerializable(typeof(ChatCompletionChunk))]
[JsonSerializable(typeof(Dictionary<string,string>))]
internal partial class JsonContext : JsonSerializerContext
{
}