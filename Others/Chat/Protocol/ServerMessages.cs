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

        public struct Packet
        {
            public ulong SequenceNumber { get; set; }
            public ulong SequenceNumberResponceTo { get; set; }
            public MessageKind Kind { get; set; }
            public object Body { get; set; }
        }
        
        public struct ErrorBody
        {
            public ErrorKind Error { get; set; }
        }

        public struct ChatBody
        {
            public string ChatContent { get; set; }
        }

        public struct InfoBody
        {
            public UserInfo Info { get; set; }
            public List<int> OnlineUsers { get; set; }
        }

        public struct NewBodyOnlineBody
        {
            public int UserId { get; set; }
            public UserInfo Info { get; set; }
        }
    }
  
}
