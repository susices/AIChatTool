using Cysharp.Threading.Tasks;

namespace AIChatTool.FunctionCalls;

public interface IFunctionCalling
{
    public static abstract  Function Function { get; }

    public static abstract UniTask<string> Handle(string args);
    
    
}