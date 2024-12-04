namespace AIChatTool;

public struct ModelInfo
{
    public string ModelName;

    public string Url;
}

public static class ModelConfig
{
    public static ModelInfo DeepSeekChat = new ModelInfo() { ModelName = "deepseek-chat", Url = "https://api.deepseek.com" };
    
    public static ModelInfo QwenPlus = new ModelInfo() { ModelName = "qwen-plus", Url = "https://dashscope.aliyuncs.com/compatible-mode/v1/" };
}