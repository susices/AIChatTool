using System.Collections.Immutable;
using System.Text.Json;
using Cysharp.Threading.Tasks;

namespace AIChatTool.FunctionCalls;

public static class FunctionCallDispatcher
{
    public static readonly ImmutableDictionary<string, IntPtr> FunctionMap;
    
    unsafe static FunctionCallDispatcher()
    {
        ImmutableDictionary<string, IntPtr>.Builder builder = ImmutableDictionary.CreateBuilder<string, IntPtr>();
        
        delegate*<string, UniTask<string>> getWeatherFunc = &GetWeather;
        builder.Add("get_weather", (IntPtr)getWeatherFunc);
        
        FunctionMap = builder.ToImmutable();
    }
    
    public static async UniTask<string> Dispatch(string functionName, string arguments)
    {
        if (!FunctionMap.TryGetValue(functionName, out var function))
        {
            Console.WriteLine($"Function {functionName} not found");
            return null;
        }
        return await InvokeFunctionCall(function,arguments);
    }


    private static unsafe UniTask<string> InvokeFunctionCall(IntPtr functionPtr,string args)
    {
        delegate*<string, UniTask<string>> functionCall = (delegate*<string, UniTask<string>>)functionPtr.ToPointer();
        return functionCall(args);
    }
   

    public static async UniTask<string> GetWeather(string args)
    {
        // args是这种格式的json文本 { "location": "天津", } 解析出location
        var json = JsonSerializer.Deserialize<Dictionary<string, string>>(args, JsonContext.Default.DictionaryStringString);
        var location = json["location"];
        return "24°C 暴风雪 不适合出门 ";
    }
}