using System.Collections.Generic;

namespace Protocol
{
    namespace Server
    {
        public enum ErrorKind : byte
        {
            InvalidId,
            RequestAfterLoggingOut
        }

        public enum MessageKind : byte
        {
            Error,
            Chat,
            Info,
            NewBodyOnline
        }

        public struct PacketHeader
        {           
            public MessageKind Kind { get; set; }
        }
        
        public struct ErrorPacket
        {
            public ErrorKind Error { get; set; }
        }

        public struct ChatPacket
        {
            public int SourceId { get; set; }
            public string ChatContent { get; set; }
        }

        public struct InfoPacket
        {
            public UserInfo Info { get; set; }
            public List<int> OnlineUsers { get; set; }
        }

        public struct NewBodyOnlinePacket
        {
            public int UserId { get; set; }
            public UserInfo Info { get; set; }
        }
    }
  
}
