using Protocol.Client;
using System;
using System.Net.Sockets;

namespace Server
{
    public class ServerEventArgs : EventArgs
    {
        public int UserId { get; private set; }
        public uint SequenceNumber { get; set; }

        public ServerEventArgs(int userId, uint sequenceNumber)
        {
            UserId = userId;
            SequenceNumber = sequenceNumber;
        }
    }

    public class ChatEventArgs : ServerEventArgs
    {        
        public int DestinationUserId { get; private set; }
        public string ChatContent { get; private set; }

        public ChatEventArgs(PacketHeader header, ChatPacket body)
            : base(header.UserId, header.SequenceNumber)
        {
            DestinationUserId = body.DestinationId;
            ChatContent = body.ChatContent;
        }
    }

    public class LogOnEventArgs : ServerEventArgs
    {
        public TcpClient Client { get; private set; }

        public LogOnEventArgs(PacketHeader header, LogOnPacket body, TcpClient client)
            : base(header.UserId, header.SequenceNumber)
        {
            Client = client;
        }
    }

    public class LogOutEventArgs : ServerEventArgs
    {
        public LogOutEventArgs(PacketHeader header, LogOutPacket body)
            : base(header.UserId, header.SequenceNumber)
        {

        }
    }

    public class HeartBeatEventArgs : ServerEventArgs
    {
        public HeartBeatEventArgs(PacketHeader header, HeartBeatPacket body)
            : base(header.UserId, header.SequenceNumber)
        {

        }
    }
}