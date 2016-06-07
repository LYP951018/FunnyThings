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

namespace Server
{
    public class ChatServer
    {
        private Timer _timer;

        public event EventHandler<ChatEventArgs> OnGotChat;
        public event EventHandler<LogOnEventArgs> OnGotLogOn;
        public event EventHandler<LogOutEventArgs> OnGotLogOut;
        public event EventHandler<HeartBeatEventArgs> OnGotHeartBeat;

        private Dictionary<int, UserRecord> _userRecords = new Dictionary<int, UserRecord>();
        private ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

        public ChatServer()
        {
            _timer = new Timer(o => ProcessHeartBeat(), null, 0, 10000);
            OnGotLogOn += ProcessLogOn;
            OnGotLogOut += ProcessLogOut;
            OnGotChat += ProcessGotChat;
            OnGotHeartBeat += ProcessGotHeartBeat;
        }

        public UserRecord QueryRecord(int userId)
        {
            UserRecord record;
            _rwLock.EnterReadLock();
            record = new UserRecord(_userRecords[userId]);
            _rwLock.ExitReadLock();
            return record;
        }

        bool HasUser(int userId)
        {
            _rwLock.EnterReadLock();
            try
            {
                return _userRecords.ContainsKey(userId);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        private UserRecord GetRecordByRef(int userId)
        {
            UserRecord record = null;
            _rwLock.EnterReadLock();
            _userRecords.TryGetValue(userId, out record);
            _rwLock.ExitReadLock();
            return record;
        }

        public void AddRecord(int userId, UserRecord record)
        {
            if(!HasUser(userId))
            {
                _rwLock.EnterWriteLock();
                _userRecords.Add(userId, record);
                _rwLock.ExitWriteLock();
            }           
        }

        private TcpClient GetClient(int userId)
        {
            UserRecord record = null;
            _rwLock.EnterReadLock();
            try
            {
                if (_userRecords.TryGetValue(userId, out record))
                    return record.Client;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return null;
        }

        public void RemoveRecord(int userId)
        {
            if(HasUser(userId))
            {
                _rwLock.EnterWriteLock();
                _userRecords.Remove(userId);
                _rwLock.ExitWriteLock();
            }               
        }

        public void SetOnlineStatus(int userId, bool isOnline)
        {
            var record = GetRecordByRef(userId);
            _rwLock.EnterWriteLock();
            record.IsOnline = isOnline;
            _rwLock.ExitWriteLock();
        }

        public void SetOnline(int userId)
        {
            SetOnlineStatus(userId, true);
        }

        public void SetOffline(int userId)
        {
            SetOnlineStatus(userId, false);
        }

        private void AddHeartBeat(int userId)
        {
            var record = GetRecordByRef(userId);
            _rwLock.EnterWriteLock();
            record.LastHeartBeatTime += TimeSpan.FromSeconds(5);
            _rwLock.ExitWriteLock();
        }

        public List<int> GetOnlineUsers()
        {
            try
            {
                _rwLock.EnterReadLock();
                {
                    return new List<int>(from v in _userRecords.Values
                                         where v.IsOnline
                                         select v.UserId);
                }
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        private void ProcessGotHeartBeat(object sender, HeartBeatEventArgs e)
        {
            AddHeartBeat(e.UserId);
        }

        private void ProcessGotChat(object sender, ChatEventArgs e)
        {
            var client = GetClient(e.DestinationUserId);
            if(client != null)
            {
                MessageHandler.SendMessage(client, new ServerProtocol.PacketHeader
                {
                    Kind = ServerProtocol.MessageKind.Chat
                },
                new ServerProtocol.ChatPacket
                {
                    SourceId = e.UserId,
                    ChatContent = e.ChatContent
                });
            }           
        }

        private void ProcessHeartBeat()
        {
            _rwLock.EnterWriteLock();
            try
            {
                foreach (var key in _userRecords.Keys)
                {
                    var record = _userRecords[key];
                    if (record.IsOnline && record.LastHeartBeatTime < DateTime.Now)
                        record.IsOnline = false;
                }
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }           
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
                    Task.Run(() => ServeForClient(client));
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
       
        private void ServeForClient(TcpClient client)
        {
            MessageHandler.ReceiveMessage(client, ProcessClientMessage);
        }

        private void ProcessLogOn(object sender, LogOnEventArgs e)
        {
            MessageHandler.SendMessage(QueryRecord(e.UserId).Client,
                new ServerProtocol.PacketHeader
                {                  
                    Kind = ServerProtocol.MessageKind.Info
                },
            new InfoPacket
            {
                Info = new UserInfo
                {
                    UserID = e.UserId
                },
                OnlineUsers = GetOnlineUsers()
            });
        }

        private void ProcessLogOut(object sender, LogOutEventArgs e)
        {
            SetOffline(e.UserId);
        }

        private void ProcessClientMessage(string headerJson, string bodyJson, TcpClient client)
        {
            var header = JsonConvert.DeserializeObject<ClientProtocol.PacketHeader>(headerJson);
            var id = header.UserId;
            AddRecord(id, new UserRecord(client, id));
            switch (header.Kind)
            {
                case ClientProtocol.MessageKind.LogOn:
                    OnGotLogOn?.Invoke(this, new LogOnEventArgs(header, JsonConvert.DeserializeObject<LogOnPacket>(bodyJson)));
                    break;
                case ClientProtocol.MessageKind.LogOut:
                    OnGotLogOut?.Invoke(this, new LogOutEventArgs(header, JsonConvert.DeserializeObject<LogOutPacket>(bodyJson)));
                    break;
                case ClientProtocol.MessageKind.Chat:
                    OnGotChat?.Invoke(this, new ChatEventArgs(header, JsonConvert.DeserializeObject<ClientProtocol.ChatPacket>(bodyJson)));
                    break;
                case ClientProtocol.MessageKind.HeartBeat:
                    OnGotHeartBeat?.Invoke(this, new HeartBeatEventArgs(header, JsonConvert.DeserializeObject<HeartBeatPacket>(bodyJson)));
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
