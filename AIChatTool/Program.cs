namespace AIChatTool;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var chatClient = new ChatClient();
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
}