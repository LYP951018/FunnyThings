using System;
using System.Net.Sockets;

namespace Server
{
    public class UserRecord
    {
        public DateTime LastHeartBeatTime { get; set; }
        public TcpClient Client { get; private set; }
        public int UserId { get; private set; }
        public bool IsOnline { get; set; }

        public UserRecord(TcpClient client, int userId, bool isOnline = true)
        {
            LastHeartBeatTime = DateTime.Now + TimeSpan.FromSeconds(10.0);
            Client = client;
            UserId = userId;
            IsOnline = isOnline;
        }

        public UserRecord(UserRecord record)
        {
            LastHeartBeatTime = record.LastHeartBeatTime;
            Client = record.Client;
            UserId = record.UserId;
            IsOnline = record.IsOnline;
        }
    }
}
