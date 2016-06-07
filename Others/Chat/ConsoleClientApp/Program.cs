using Client;
using System;
using System.Text.RegularExpressions;

namespace ConsoleChatApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Input ID!");
            var idStr = Console.ReadLine();
            var id = Int32.Parse(idStr);
            var client = new ChatClient(id);
            client.OnGotInfo += (o, e) =>
            {
                foreach (var f in e.OnlineUsers)
                    Console.WriteLine(f);
            };
            client.OnGotMessage += (o, e) =>
                Console.WriteLine($"Got message fron {e.SourceId}! {e.ChatContent}");
            client.Start();
            client.LogOn();
            while (true)
            {
                var input = Console.ReadLine();
                try
                {
                    var m = Regex.Match(input, @"^\s*(?<command>\S*)");
                    if (m.Success)
                    {
                        var collection = m.Groups["command"];
                        switch (collection.ToString())
                        {
                            case "sendto":
                                var subStr = input.Substring(m.Index + "sendto".Length);
                                var m2 = Regex.Match(subStr, @"\s*(?<id>\d+)\s+(?<message>.*)");
                                if (m2.Success)
                                {
                                    client.Chat(Int32.Parse(m2.Groups["id"].ToString()), m2.Groups["message"].ToString());
                                }
                                break;
                            default:
                                Console.WriteLine("Unsupported!");
                                break;
                        }
                    }
                }
                finally { }
            }
        }
    }
}
