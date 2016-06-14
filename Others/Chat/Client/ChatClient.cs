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
using System.Collections.Generic;

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

    public sealed class DuplicateIdException : Exception
    {
        public DuplicateIdException(string message = null, Exception innerException = null)
            : base(message, innerException)
        {

        }

        public override string Message => $"{base.Message} No such ID!";
    }

    public class ChatClient
    {
        private Timer _timer;
        private int _userId;

        public int UserId => _userId;

        private TcpClient _client = new TcpClient();
        private volatile uint _sequenceNumber = 0;

        public event EventHandler<ChatEventArgs> OnGotMessage;
        public event EventHandler<InfoEventArgs> OnGotInfo;
        public event EventHandler<ErrorEventArgs> OnGotError;
        public event EventHandler<NewBodyOnlineEventArgs> OnGotNewBody;

        private class LogOnToken
        {
            public uint SequenceNumber { get; }
            public TaskCompletionSource<LogOnRequestResult> TheTask { get; }

            public LogOnToken(uint sequenceNumber)
            {
                SequenceNumber = sequenceNumber;
                TheTask = new TaskCompletionSource<LogOnRequestResult>();
            }
        }

        private LogOnToken _logOnToken = null;

        private void ChatClient_OnGotInfo(object sender, InfoEventArgs e)
        {
            _timer = new Timer(x => SendHeartBeat(), null, 0, 5000);
            Console.WriteLine("Got info!");
            _logOnToken.TheTask.SetResult(new LogOnRequestResult(ErrorKind.Ok, e.Info, e.OnlineUsers));
            //_logOnToken = null;
        }

        private void ChatClient_OnGotError(object sender, ErrorEventArgs e)
        {
            Exception exception;

            switch(e.Error)
            {
                case ErrorKind.DuplicateId:
                    exception = new DuplicateIdException();
                    break;
                default:
                    throw new InvalidOperationException();
            }
            _logOnToken.TheTask.SetException(exception);
           // _logOnToken = null;         
        }

        public ChatClient()
        {
            OnGotError += ChatClient_OnGotError;
            OnGotInfo += ChatClient_OnGotInfo;           
        }

        private uint GenerateSequenceNumber()
        {
            var prev = _sequenceNumber;
            ++_sequenceNumber;
            return prev;
        }

        public Task<LogOnRequestResult> LogOn(int userId)
        {
            _userId = userId;
            var number = GenerateSequenceNumber();
            MessageHandler.SendMessage(_client, new ClientProtocol.PacketHeader
            (
                _userId,
                ClientProtocol.MessageKind.LogOn,
                number
            ),
            new LogOnPacket());
            _logOnToken = new LogOnToken(number);
            return _logOnToken.TheTask.Task;
        }

        //public Task<LogOnPacket>

        public void Chat(int desId, string message)
        {
            MessageHandler.SendMessage(_client,
                new ClientProtocol.PacketHeader(_userId, ClientProtocol.MessageKind.Chat, GenerateSequenceNumber()),
                new ClientProtocol.ChatPacket(desId, message));
        }

        public void LogOut()
        {
            MessageHandler.SendMessage(_client, new ClientProtocol.PacketHeader
            (
                _userId,
                ClientProtocol.MessageKind.LogOut,
                GenerateSequenceNumber()
            ),
            new LogOutPacket());
        }

        private void SendHeartBeat()
        {
            MessageHandler.SendMessage(_client,
               new ClientProtocol.PacketHeader
               (
                   _userId,
                   ClientProtocol.MessageKind.HeartBeat,
                   GenerateSequenceNumber()          
               ), new HeartBeatPacket());
        }

        private void ProcessServerMessage(string headerJson, string bodyJson, TcpClient client)
        {
            var header = JsonConvert.DeserializeObject<ServerProtocol.PacketHeader>(headerJson);
            if (header.Error != ErrorKind.Ok)
            {
                OnGotError?.Invoke(this, new ErrorEventArgs(header));
                return;
            }
            switch (header.Kind)
            {
                case ServerProtocol.MessageKind.Chat:            
                    OnGotMessage?.Invoke(this, new ChatEventArgs(header, JsonConvert.DeserializeObject<ServerProtocol.ChatPacket>(bodyJson)));                   
                    break;
                case ServerProtocol.MessageKind.Info:
                    OnGotInfo?.Invoke(this, new InfoEventArgs(header, JsonConvert.DeserializeObject<InfoPacket>(bodyJson)));
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
