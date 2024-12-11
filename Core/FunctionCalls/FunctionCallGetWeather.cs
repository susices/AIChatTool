using System.Text.Json;
using Cysharp.Threading.Tasks;

namespace AIChatTool.FunctionCalls;

public class FunctionCallGetWeather : IFunctionCalling
{
    public static Function Function
    {
        get
        {
            return new Function()
            {
                Name = "get_weather",
                Description = "Get weather of an location, the user shoud supply a location first",
                Parameters = new Parameters
                {
                    Type = "object",
                    Properties = new Dictionary<string, Property>
                    {
                        {
                            "location",
                            new Property()
                            {
                                Type = "string",
                                Description = "The city and state, e.g. San Francisco, CA"
                            }
                        }
                    },
                    Required = new List<string>()
                    {
                        "location"
                    }
                }
            };
        }
    }

    public static async UniTask<string> Handle(string args)
    {
        await UniTask.CompletedTask;
        var json = JsonSerializer.Deserialize<Dictionary<string, string>>(args, JsonContext.Default.DictionaryStringString);
        var location = json["location"];
        return "24°C 暴风雪 不适合出门 ";
    }
}