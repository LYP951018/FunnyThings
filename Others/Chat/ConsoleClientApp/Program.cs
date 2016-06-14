using Client;
using Protocol.Server;
using System;
using System.Text.RegularExpressions;

namespace ConsoleChatApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
           
            var client = new ChatClient();
            client.OnGotInfo += (o, e) =>
            {
                foreach (var f in e.OnlineUsers)
                    Console.WriteLine(f);
            };
            client.OnGotMessage += (o, e) =>
                Console.WriteLine($"Got message fron {e.SourceId}! {e.ChatContent}");
            client.OnGotError += (o, e) =>
            {
                if (e.Error == ErrorKind.DuplicateId)
                    Console.WriteLine("Duplicated ID!");
            };
            client.Start();
            while(true)
            {
                try
                {
                    Console.WriteLine("Input ID!");
                    var idStr = Console.ReadLine();
                    var id = Int32.Parse(idStr);
                    client.LogOn(id).Wait();
                    break;
                }
                catch(AggregateException e)
                {
                    e.Handle(ex => ex is DuplicateIdException);
                }
            }
            
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
