using System;
using System.Collections.Generic;
using Protocol.Server;
using Protocol;

namespace Client
{
    public class ClientEventArgs : EventArgs
    {
        public MessageKind Kind { get; private set; }

        private ClientEventArgs(MessageKind kind)
        {
            Kind = kind;
        }

        public ClientEventArgs(PacketHeader header)
            : this(header.Kind)
        { 
        }
    }

    public class ChatEventArgs : ClientEventArgs
    {
        public int SourceId { get; private set; }
        public string ChatContent { get; private set; }

        public ChatEventArgs(PacketHeader header, ChatPacket body)
            : base(header)
        {
            SourceId = body.SourceId;
            ChatContent = body.ChatContent;
        }
    }

    public class InfoEventArgs : ClientEventArgs
    {
        public UserInfo Info { get; private set;}
        public List<int> OnlineUsers { get; private set; }

        public InfoEventArgs(PacketHeader header, InfoPacket body)
            : base(header)
        {
            Info = body.Info;
            OnlineUsers = body.OnlineUsers;
        }
    }

    public class ErrorEventArgs : ClientEventArgs
    {
        public ErrorKind Error { get; private set; }

        public ErrorEventArgs(PacketHeader header)
            : base(header)
        {
            Error = header.Error;
        }
    }

    public class NewBodyOnlineEventArgs : ClientEventArgs
    {
        public int NewUserId { get; private set; }
        public UserInfo NewUserInfo { get; private set; }

        public NewBodyOnlineEventArgs(PacketHeader header, NewBodyOnlinePacket body)
            : base(header)
        {
            NewUserId = body.UserId;
            NewUserInfo = body.Info;
        }
    }
}
