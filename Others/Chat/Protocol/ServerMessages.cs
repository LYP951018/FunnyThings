using System.Collections.Generic;

namespace Protocol
{
    namespace Server
    {
        public enum ErrorKind : byte
        {
            Ok,
            InvalidId,
            RequestAfterLoggingOut,
            DuplicateId
        }

        public enum MessageKind : byte
        {
            Chat,
            Info,
            NewBodyOnline
        }

        public class PacketHeader
        {           
            public MessageKind Kind { get;}
            public ErrorKind Error { get; }
            public uint SequenceNumberResponseTo { get; }

            public PacketHeader(MessageKind kind, ErrorKind error, uint responseTo)
            {
                Kind = kind;
                Error = error;
                SequenceNumberResponseTo = responseTo;
            }
        }

        public class ChatPacket
        {
            public int SourceId { get; }
            public string ChatContent { get; }

            public ChatPacket(int sourceId, string chatContent)
            {
                SourceId = sourceId;
                ChatContent = chatContent;
            }
        }

        public class InfoPacket
        {
            public UserInfo Info { get; }
            public List<int> OnlineUsers { get; }
            
            public InfoPacket(UserInfo info, IEnumerable<int> onlineUsers)
            {
                Info = info;
                OnlineUsers = onlineUsers == null ? null : new List<int>(onlineUsers);
            }
        }

        public class NewBodyOnlinePacket
        {
            public int UserId { get; }
            public UserInfo Info { get; }

            public NewBodyOnlinePacket(int userId, UserInfo info)
            {
                UserId = userId;
                Info = info;
            }
        }
    }
  
}
