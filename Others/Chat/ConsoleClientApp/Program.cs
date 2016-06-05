using Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                Console.WriteLine("balala");
            };
            Task.Run(() => client.StartAsync());
            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
