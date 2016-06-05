using Protocol;
using Protocol.Client;
using Protocol.Server;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Linq;

using ServerProtocol = Protocol.Server;
using ClientProtocol = Protocol.Client;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Server
{
    public class ChatServer
    {
        class UserRecord
        {
            public DateTime LastHeartBeatTime { get; set; }
            public IPAddress UserAddress { get; set; }
        }

        private Timer _timer;
        private MessageHandler _messageHandler = new MessageHandler(false);
        //private Dictionary<int, UserInfo> _usersInfo = new Dictionary<int, UserInfo>();
        private Dictionary<int, UserRecord> _onlineUsers = new Dictionary<int, UserRecord>();
        private SemaphoreSlim _infoSync = new SemaphoreSlim(1, 1);
        //private SemaphoreSlim _onlineSync = new SemaphoreSlim(1, 1);

        public event EventHandler<PayloadEventArgs<PayloadPacket<ClientProtocol.ChatBody>>> OnGotChat;
        public event EventHandler<PayloadEventArgs<PayloadPacket<LogOnBody>>> OnGotLogOn;
        public event EventHandler<PayloadEventArgs<PayloadPacket<LogOutBody>>> OnGotLogOut;
        public event EventHandler<PayloadEventArgs<PayloadPacket<HeartBeatBody>>> OnGotHeartBeat;

        public ChatServer()
        {
            _timer = new Timer(o => ProcessHeartBeat(), null, 0, 10000);
            OnGotLogOn += ProcessLogOn;
            OnGotLogOut += ProcessLogOut;
            OnGotChat += ProcessGotChat;
            OnGotHeartBeat += ProcessGotHeartBeat;
        }

        private async void ProcessGotHeartBeat(object sender, PayloadEventArgs<PayloadPacket<HeartBeatBody>> e)
        {
            Console.WriteLine("Got heartbeat!");
            var id = e.Body.Body.UserId;
            await _infoSync.WaitAsync();
            try
            {
                if (_onlineUsers.ContainsKey(id))
                    _onlineUsers[id].LastHeartBeatTime += TimeSpan.FromSeconds(10000);
            }
            finally
            {
                _infoSync.Release();
            }            
        }

        private void ProcessGotChat(object sender, PayloadEventArgs<PayloadPacket<ClientProtocol.ChatBody>> e)
        {
            var message = e.Body;
            SendServerMessage(message.UserData as IPAddress, new ServerProtocol.Packet
            {
                Kind = ServerProtocol.MessageKind.Chat,
                Body = new ServerProtocol.ChatBody
                {
                    ChatContent = message.Body.ChatContent
                }
            });
        }

        private async void ProcessHeartBeat()
        {
            await _infoSync.WaitAsync();
            try
            {
                var userToRemove = _onlineUsers.Where(kv => DateTime.Now > kv.Value.LastHeartBeatTime).ToArray();
                foreach (var u in userToRemove)
                    _onlineUsers.Remove(u.Key);
            }
            finally
            {
                _infoSync.Release();
            }           
        }

        /// <summary>
        /// Uses reflection to get the field value from an object.
        /// </summary>
        ///
        /// <param name="type">The instance type.</param>
        /// <param name="instance">The instance object.</param>
        /// <param name="fieldName">The field's name which is to be fetched.</param>
        ///
        /// <returns>The field value from the object.</returns>
        internal static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }

        public void Start()
        {
            var listener = new TcpListener(IPAddress.Any, Config.ServerPortNumber);
            listener.Start();
            try
            {
                while (true)
                {
                    Console.WriteLine("Listening.");
                    var client = listener.AcceptTcpClientAsync().Result;                   
                    var clientAddress = ((GetInstanceField(typeof(TcpClient), client, "_clientSocket") as Socket).RemoteEndPoint as IPEndPoint).Address;
                    Console.WriteLine($"Accepted from {clientAddress}");
                    Task.Run(async () =>
                        ProcessClientMessage(await RestorePacket(await StringReader.Read(client.GetStream()))
                        , clientAddress));
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                listener.Stop();
            }           
        }

        private async Task<ClientProtocol.Packet> RestorePacket(string json)
        {
            return await Task.Run(() =>
            {
                var packet = JsonConvert.DeserializeObject<ClientProtocol.Packet>(json);
                var jBody = packet.Body as JObject;
                switch (packet.Kind)
                {
                    case ClientProtocol.MessageKind.Chat:
                        packet.Body = jBody.ToObject<ClientProtocol.ChatBody>();
                        break;
                    case ClientProtocol.MessageKind.HeartBeat:
                        packet.Body = jBody.ToObject<HeartBeatBody>();
                        break;
                    case ClientProtocol.MessageKind.LogOn:
                        packet.Body = jBody.ToObject<LogOnBody>();
                        break;
                    case ClientProtocol.MessageKind.LogOut:
                        packet.Body = jBody.ToObject<LogOutBody>();
                        break;
                }
                return packet;
            });
        }

        private async void ProcessLogOn(object sender, PayloadEventArgs<PayloadPacket<LogOnBody>> e)
        {
            Console.WriteLine("LogOn!");
            var body = e.Body;
            var address = body.UserData as IPAddress;
            await _infoSync.WaitAsync();
            try
            {
                _onlineUsers.Add(body.Body.UserId, new UserRecord
                {
                    LastHeartBeatTime = DateTime.Now + TimeSpan.FromSeconds(2),
                    UserAddress = (IPAddress)body.UserData
                });
            }
            finally
            {
                _infoSync.Release();
            }

            SendServerMessage(address, new ServerProtocol.Packet
            {
                SequenceNumberResponceTo = body.Header.SequenceNumber,
                Kind = ServerProtocol.MessageKind.Info,
                Body = new InfoBody()
            });
        }

        private async void ProcessLogOut(object sender, PayloadEventArgs<PayloadPacket<LogOutBody>> e)
        {
            var body = e.Body;
            var id = body.Body.UserId;
            await _infoSync.WaitAsync();
            try
            {
                if (_onlineUsers.ContainsKey(id))
                    _onlineUsers.Remove(id);
                else
                    goto SendError;
            }
            finally
            {
                _infoSync.Release();
            }
            SendError:
            SendServerMessage(body.UserData as IPAddress, new ServerProtocol.Packet
            {
                SequenceNumberResponceTo = body.Header.SequenceNumber,
                Kind = ServerProtocol.MessageKind.Error,
                Body = new ErrorBody
                {
                    Error = ErrorKind.RequestAfterLoggingOut
                }
            });
        }

        private async void SendServerMessage(IPAddress address, ServerProtocol.Packet sm)
        {
            await _messageHandler.SendMessage(address, sm);
        }

        private void ProcessClientMessage(ClientProtocol.Packet message, IPAddress address)
        {
            switch (message.Kind)
            {
                case ClientProtocol.MessageKind.LogOn:
                    OnGotLogOn?.Invoke(this, new PayloadEventArgs<PayloadPacket<LogOnBody>>(new PayloadPacket<LogOnBody>(message, address)));
                    break;
                case ClientProtocol.MessageKind.LogOut:
                    OnGotLogOut?.Invoke(this, new PayloadEventArgs<PayloadPacket<LogOutBody>>(new PayloadPacket<LogOutBody>(message, address)));
                    break;
                case ClientProtocol.MessageKind.Chat:
                    OnGotChat?.Invoke(this, new PayloadEventArgs<PayloadPacket<ClientProtocol.ChatBody>>(new PayloadPacket<ClientProtocol.ChatBody>(message, address)));
                    break;
                case ClientProtocol.MessageKind.HeartBeat:
                    OnGotHeartBeat?.Invoke(this, new PayloadEventArgs<PayloadPacket<HeartBeatBody>>(new PayloadPacket<HeartBeatBody>(message, address)));
                    break;
            }
        }
    }
}
