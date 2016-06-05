using Server;

namespace ServerApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var server = new ChatServer();
            server.Start();
        }
    }
}
