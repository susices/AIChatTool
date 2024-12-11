using System.Collections.Immutable;
using System.Text.Json;
using Cysharp.Threading.Tasks;

namespace AIChatTool.FunctionCalls;

public static class FunctionCallDispatcher
{
    private static readonly Dictionary<string, IntPtr> FunctionMap = new Dictionary<string, IntPtr>();
    
    static FunctionCallDispatcher()
    {
        RegisterFunctionCall<FunctionCallGetWeather>();
    }
    
    public static UniTask<string> Dispatch(string functionName, string arguments)
    {
        if (!FunctionMap.TryGetValue(functionName, out var function))
        {
            Console.WriteLine($"Function {functionName} not found");
            return UniTask.FromResult(string.Empty);
        }
        return InvokeFunctionCall(function,arguments);
    }

    private static unsafe UniTask<string> InvokeFunctionCall(IntPtr functionPtr,string args)
    {
        delegate*<string, UniTask<string>> functionCall = (delegate*<string, UniTask<string>>)functionPtr.ToPointer();
        return functionCall(args);
    }

    public static unsafe void RegisterFunctionCall<T>()where T : IFunctionCalling
    {
        delegate*<string, UniTask<string>> getWeatherFunc = &T.Handle;
        FunctionMap.Add(T.Function.Name, (IntPtr)getWeatherFunc);
    }
}