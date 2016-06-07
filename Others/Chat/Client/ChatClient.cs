using Newtonsoft.Json;
using Protocol;
using Protocol.Client;
using Protocol.Server;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using ServerProtocol = Protocol.Server;
using ClientProtocol = Protocol.Client;

namespace Client
{
    public sealed class RequestAfterLoggingOutException : Exception
    {
        public RequestAfterLoggingOutException(string message = null, Exception innerException = null)
            : base(message, innerException)
        {

        }

        public override string Message => $"{base.Message} Send request after logging out!";        
    }

    public sealed class InvalidIdException : Exception
    {
        public InvalidIdException(string message = null, Exception innerException = null)
            : base(message, innerException)
        {

        }

        public override string Message => $"{base.Message} No such ID!";
    }

    public class ChatClient
    {
        private Timer _timer;
        private int _userId;
        private TcpClient _client = new TcpClient();

        public event EventHandler<ChatEventArgs> OnGotMessage;
        public event EventHandler<InfoEventArgs> OnGotInfo;
        public event EventHandler<ErrorEventArgs> OnGotError;
        public event EventHandler<NewBodyOnlineEventArgs> OnGotNewBody;

        public ChatClient(int id)
        {
            _userId = id;
            OnGotInfo += (o, e) => _timer = new Timer(x => SendHeartBeat(), null, 0, 10000);
            OnGotInfo += (o, e) => Console.WriteLine("Got info!");
        }

        public void LogOn()
        {
            MessageHandler.SendMessage(_client, new ClientProtocol.PacketHeader
            {
                UserId = _userId,
                Kind = ClientProtocol.MessageKind.LogOn
            },
            new LogOnPacket());
        }

        public void Chat(int desId, string message)
        {
            MessageHandler.SendMessage(_client,
                new ClientProtocol.PacketHeader
                {
                    UserId = _userId,
                    Kind = ClientProtocol.MessageKind.Chat,                    
                },
                new ClientProtocol.ChatPacket
                {
                    DestinationId = desId,
                    ChatContent = message
                });
        }

        public void LogOut()
        {
            MessageHandler.SendMessage(_client, new ClientProtocol.PacketHeader
            {
                UserId = _userId,
                Kind = ClientProtocol.MessageKind.LogOut
            },
            new LogOutPacket());
        }

        private void SendHeartBeat()
        {
            MessageHandler.SendMessage(_client,
               new ClientProtocol.PacketHeader
               {
                   UserId = _userId,
                   Kind = ClientProtocol.MessageKind.HeartBeat,                   
               }, new HeartBeatPacket());
        }

        private void ProcessServerMessage(string headerJson, string bodyJson, TcpClient client)
        {
            var header = JsonConvert.DeserializeObject<ServerProtocol.PacketHeader>(headerJson);
            switch (header.Kind)
            {
                case ServerProtocol.MessageKind.Chat:                   
                    OnGotMessage?.Invoke(this, new ChatEventArgs(header, JsonConvert.DeserializeObject<ServerProtocol.ChatPacket>(bodyJson)));                   
                    break;
                case ServerProtocol.MessageKind.Info:
                    OnGotInfo?.Invoke(this, new InfoEventArgs(header, JsonConvert.DeserializeObject<InfoPacket>(bodyJson)));
                    break;
                case ServerProtocol.MessageKind.Error:
                    OnGotError?.Invoke(this, new ErrorEventArgs(header, JsonConvert.DeserializeObject<ErrorPacket>(bodyJson)));                    
                    break;
                case ServerProtocol.MessageKind.NewBodyOnline:
                    OnGotNewBody?.Invoke(this, new NewBodyOnlineEventArgs(header, JsonConvert.DeserializeObject<NewBodyOnlinePacket>(bodyJson)));
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
        
        public async Task StartAsync()
        {
            await _client.ConnectAsync(IPAddress.Parse(Config.ServerIPAddress), Config.ServerPortNumber);
            await Task.Run(() => MessageHandler.ReceiveMessage(_client, ProcessServerMessage));               
        }

        public void Start()
        {
            _client.ConnectAsync(IPAddress.Parse(Config.ServerIPAddress), Config.ServerPortNumber).Wait();
            MessageHandler.ReceiveMessage(_client, ProcessServerMessage);
        }
    }
}
