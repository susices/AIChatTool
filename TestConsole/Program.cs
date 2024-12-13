

using AIChatTool;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            // 自备apikey
            string apiKey = null;
            var chatClient = new ChatClient(ModelConfig.QwenPlus, apiKey);
            {
                var res = await chatClient.SendChatMsg("你好");
                //Console.WriteLine(res);
            }

            while (true)
            {
                var msg = Console.ReadLine();
                var res = await chatClient.SendChatMsg(msg);
                //Console.WriteLine(res);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}