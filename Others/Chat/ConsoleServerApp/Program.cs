using Server;
using System;

namespace ServerApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var server = new ChatServer();
            server.OnGotChat += (o, e) => Console.WriteLine($"{e.UserId} sends message to {e.DestinationUserId} {e.ChatContent}");
            server.OnGotLogOn += (o, e) => Console.WriteLine($"{e.UserId} log on!");
            server.Start();
        }
    }
}
