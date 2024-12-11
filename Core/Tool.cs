using System.Text.Json.Serialization;

namespace AIChatTool;

public class Tool
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("function")]
    public Function Function { get; set; }
}

public class Function
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("parameters")]
    public Parameters Parameters { get; set; }
}

public class Parameters
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("properties")]
    public Dictionary<string, Property> Properties { get; set; }

    [JsonPropertyName("required")]
    public List<string> Required { get; set; }
}

public class Property
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
}