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
            _timer = new Timer(o => ProcessHeartBeat(), null, 0, 9000);
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

        public void AddRecord(int userId, UserRecord record)
        {
            _rwLock.EnterWriteLock();
            if (!_userRecords.ContainsKey(userId))
                _userRecords.Add(userId, record);
            _rwLock.ExitWriteLock();
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
                Console.WriteLine($"{userId} log off.");
            }               
        }

        //public void SetOnlineStatus(int userId, bool isOnline)
        //{
        //    var record = GetRecordByRef(userId);
        //    _rwLock.EnterWriteLock();
        //    record.IsOnline = isOnline;
        //    _rwLock.ExitWriteLock();
        //}

        //public void SetOnline(int userId)
        //{
        //    SetOnlineStatus(userId, true);
        //}

        //public void SetOffline(int userId)
        //{
        //    SetOnlineStatus(userId, false);
        //}

        private void AddHeartBeat(int userId)
        {           
            _rwLock.EnterWriteLock();
            UserRecord record = null;
            _userRecords.TryGetValue(userId, out record);
            if(record != null)
                record.LastHeartBeatTime += TimeSpan.FromSeconds(5);
            _rwLock.ExitWriteLock();
        }

        public List<int> GetOnlineUsers()
        {
            try
            {
                _rwLock.EnterReadLock();
                {
                    return new List<int>(_userRecords.Keys);
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
                (
                    ServerProtocol.MessageKind.Chat,
                    ErrorKind.Ok,
                    e.SequenceNumber
                ),
                new ServerProtocol.ChatPacket
                (
                    e.UserId,
                    e.ChatContent
                ));
            }           
        }

        private void ProcessHeartBeat()
        {
            _rwLock.EnterWriteLock();
            try
            {
                var keysToRemove = new List<int>();
                foreach (var key in _userRecords.Keys)
                {
                    var record = _userRecords[key];
                    var now = DateTime.Now;
                    if (/*record.IsOnline && */record.LastHeartBeatTime < now)
                        //record.IsOnline = false;
                        keysToRemove.Add(key);
                }
                foreach (var k in keysToRemove)
                {
                    _userRecords.Remove(k);
                    Console.WriteLine($"{k} is offline.");
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
            var id = e.UserId;
            AddRecord(id, new UserRecord(e.Client, id));
            MessageHandler.SendMessage(QueryRecord(e.UserId).Client,
                new ServerProtocol.PacketHeader(ServerProtocol.MessageKind.Info, ErrorKind.Ok, e.SequenceNumber),
            new InfoPacket(new UserInfo { UserID = e.UserId }, GetOnlineUsers()));
            _rwLock.EnterReadLock();
            foreach(var user in _userRecords)
            {
                var record = user.Value;
                if (user.Key != e.UserId)
                {
                    Console.WriteLine($"Told {user.Key}  {e.UserId} online!");
                    MessageHandler.SendMessage(record.Client, new ServerProtocol.PacketHeader(ServerProtocol.MessageKind.NewBodyOnline, ErrorKind.Ok, 0),
                       new NewBodyOnlinePacket(e.UserId, new UserInfo { UserID = e.UserId }));
                }
                   
            }
            _rwLock.ExitReadLock();
        }

        private void ProcessLogOut(object sender, LogOutEventArgs e)
        {
            RemoveRecord(e.UserId);
        }

        private bool Validate(TcpClient client, int userId)
        {
            _rwLock.EnterReadLock();
            try
            {
                return client == _userRecords[userId].Client;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        private void ProcessClientMessage(string headerJson, string bodyJson, TcpClient client)
        {
            var header = JsonConvert.DeserializeObject<ClientProtocol.PacketHeader>(headerJson);
            var id = header.UserId;
            switch (header.Kind)
            {
                case ClientProtocol.MessageKind.LogOn:
                    {
                        if (HasUser(id))
                        {
                            //TODO: 发送 null。
                            MessageHandler.SendMessage(client, new ServerProtocol.PacketHeader(ServerProtocol.MessageKind.Info, ErrorKind.DuplicateId, header.SequenceNumber), null);
                            return;
                        }
                        OnGotLogOn?.Invoke(this, new LogOnEventArgs(header, JsonConvert.DeserializeObject<LogOnPacket>(bodyJson), client));
                    }
                    break;
                case ClientProtocol.MessageKind.LogOut:
                    if (Validate(client, id))
                        OnGotLogOut?.Invoke(this, new LogOutEventArgs(header, JsonConvert.DeserializeObject<LogOutPacket>(bodyJson)));
                    break;
                case ClientProtocol.MessageKind.Chat:
                    var body = JsonConvert.DeserializeObject<ClientProtocol.ChatPacket>(bodyJson);
                    OnGotChat?.Invoke(this, new ChatEventArgs(header, body));
                    break;
                case ClientProtocol.MessageKind.HeartBeat:
                    if(Validate(client, id))
                        OnGotHeartBeat?.Invoke(this, new HeartBeatEventArgs(header, JsonConvert.DeserializeObject<HeartBeatPacket>(bodyJson)));
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
