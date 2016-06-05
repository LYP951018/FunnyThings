using Newtonsoft.Json;
using Protocol;
using Protocol.Client;
using Protocol.Server;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using ServerProtocol = Protocol.Server;
using ClientProtocol = Protocol.Client;
using Newtonsoft.Json.Linq;

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
        private MessageHandler _messageHandler = new MessageHandler(true);
        private Dictionary<ulong, ClientProtocol.Packet> _messages = new Dictionary<ulong, ClientProtocol.Packet>();
        public Dictionary<ulong, ClientProtocol.Packet> Messages => _messages;
        private object _sync = new object();

        public event EventHandler<PayloadEventArgs<ServerProtocol.ChatBody>> OnGotMessage;
        public event EventHandler<PayloadEventArgs<ServerProtocol.InfoBody>> OnGotInfo;
        public event EventHandler<PayloadEventArgs<ServerProtocol.ErrorBody>> OnGotError;
        public event EventHandler<PayloadEventArgs<ServerProtocol.NewBodyOnlineBody>> OnGotNewBody;

        public ChatClient(int id)
        {
            _userId = id;
            OnGotInfo += (o, e) => _timer = new Timer(x => SendHeartBeat(), null, 0, 10000);
            OnGotInfo += (o, e) => Console.WriteLine("Got info!");
        }

        public void LogOn()
        {
            SendClientMessage(
                new ClientProtocol.Packet
                {
                    Kind = ClientProtocol.MessageKind.LogOn,
                    Body = new ClientProtocol.LogOnBody
                    {
                        UserId = _userId,
                    }
                });
        }

        private async void SendClientMessage(ClientProtocol.Packet cm)
        {
            await _messageHandler.SendMessage(IPAddress.Parse(Config.ServerIPAddress),
                cm).ContinueWith(t =>
            {
                lock(_sync)
                {
                    _messages.Add(t.Result, cm);
                }               
            });
        }

        public void SendMessage(string message, int desId)
        {
            SendClientMessage(
                new ClientProtocol.Packet
                {
                    Kind = ClientProtocol.MessageKind.Chat,                    
                    Body = new ClientProtocol.ChatBody
                    {
                        UserId = _userId,
                        DestinationId = desId,
                        ChatContent = message
                    }
                });
        }

        public void LogOut()
        {
            SendClientMessage(
                new ClientProtocol.Packet
                {
                    Kind = ClientProtocol.MessageKind.LogOut,
                    Body = new ClientProtocol.LogOutBody
                    {
                        UserId = _userId,
                    }
                });
        }

        private void SendHeartBeat()
        {
            SendClientMessage(
               new ClientProtocol.Packet
               {
                   Kind = ClientProtocol.MessageKind.HeartBeat,
                   Body = new ClientProtocol.HeartBeatBody
                   {
                       UserId = _userId,
                   }
               });
        }

        public void ProcessServerMessage(ServerProtocol.Packet message)
        {
            var responseTo = message.SequenceNumberResponceTo;
            Messages.Remove(responseTo);
            switch (message.Kind)
            {
                case ServerProtocol.MessageKind.Chat:                   
                    OnGotMessage?.Invoke(this, new PayloadEventArgs<ServerProtocol.ChatBody>((ServerProtocol.ChatBody)message.Body));                   
                    break;
                case ServerProtocol.MessageKind.Info:
                    OnGotInfo?.Invoke(this, new PayloadEventArgs<ServerProtocol.InfoBody>((ServerProtocol.InfoBody)message.Body));
                    break;
                case ServerProtocol.MessageKind.Error:
                    OnGotError?.Invoke(this, new PayloadEventArgs<ServerProtocol.ErrorBody>((ServerProtocol.ErrorBody)message.Body));                    
                    break;
                case ServerProtocol.MessageKind.NewBodyOnline:
                    OnGotNewBody?.Invoke(this, new PayloadEventArgs<ServerProtocol.NewBodyOnlineBody>((ServerProtocol.NewBodyOnlineBody)message.Body));
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
        
        public async void StartAsync()
        {
            var listener = new TcpListener(IPAddress.Any, Config.ClientPortNumber);
            var buffer = new byte[1024];
            listener.Start();
            LogOn();
            
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                ProcessServerMessage(await RestorePacket(await StringReader.Read(client.GetStream())));
            }
        }

        private async Task<ServerProtocol.Packet> RestorePacket(string json)
        {
            return await Task.Run(() =>
            {
                var packet = JsonConvert.DeserializeObject<ServerProtocol.Packet>(json);
                var jBody = packet.Body as JObject;
                switch (packet.Kind)
                {
                    case ServerProtocol.MessageKind.Error:
                        packet.Body = jBody.ToObject<ErrorBody>();
                        break;
                    case ServerProtocol.MessageKind.Info:
                        packet.Body = jBody.ToObject<InfoBody>();
                        break;
                    case ServerProtocol.MessageKind.NewBodyOnline:
                        packet.Body = jBody.ToObject<NewBodyOnlineBody>();
                        break;
                    case ServerProtocol.MessageKind.Chat:
                        packet.Body = jBody.ToObject<ServerProtocol.ChatBody>();
                        break;
                }
                return packet;
            });
        }

        public async void Start()
        {
            var listener = new TcpListener(IPAddress.Any, Config.ClientPortNumber);
            var buffer = new byte[1024];
            while (true)
            {
                var client = listener.AcceptTcpClientAsync().Result;
                ProcessServerMessage(
                    await Task.Run(async ()
                    => (ServerProtocol.Packet)JsonConvert.DeserializeObject(await StringReader.Read(client.GetStream()))));
            }
        }
    }
}
