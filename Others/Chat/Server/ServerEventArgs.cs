using Protocol.Client;
using System;

namespace Server
{
    public class ServerEventArgs : EventArgs
    {
        public int UserId { get; private set; }

        public ServerEventArgs(int userId)
        {
            UserId = userId;
        }
    }

    public class ChatEventArgs : ServerEventArgs
    {        
        public int DestinationUserId { get; private set; }
        public string ChatContent { get; private set; }

        public ChatEventArgs(PacketHeader header, ChatPacket body)
            : base(header.UserId)
        {
            DestinationUserId = body.DestinationId;
            ChatContent = body.ChatContent;
        }
    }

    public class LogOnEventArgs : ServerEventArgs
    {
        public LogOnEventArgs(PacketHeader header, LogOnPacket body)
            : base(header.UserId)
        {

        }
    }

    public class LogOutEventArgs : ServerEventArgs
    {
        public LogOutEventArgs(PacketHeader header, LogOutPacket body)
            : base(header.UserId)
        {

        }
    }

    public class HeartBeatEventArgs : ServerEventArgs
    {
        public HeartBeatEventArgs(PacketHeader header, HeartBeatPacket body)
            : base(header.UserId)
        {

        }
    }
}