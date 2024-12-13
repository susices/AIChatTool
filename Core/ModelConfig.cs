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
    
    public static ModelInfo Gemini_exp_1206 = new ModelInfo() { ModelName = "gemini-exp-1206", Url = "https://generativelanguage.googleapis.com/v1beta/openai/" };
    
    public static ModelInfo TestLocalAgent = new ModelInfo() { ModelName = "qwen-plus", Url = "http://localhost:5206" };
}