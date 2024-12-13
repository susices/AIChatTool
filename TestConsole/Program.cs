

using AIChatTool;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            var chatClient = new ChatClient(ModelConfig.TestLocalAgent);
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