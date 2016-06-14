namespace Protocol
{
    namespace Client
    {
        public enum MessageKind : byte
        {
            LogOn,
            LogOut,
            Chat,
            HeartBeat
        }
       
        public class PacketHeader
        {
            public int UserId { get; }
            public MessageKind Kind { get; }  
            public uint SequenceNumber { get; }

            public PacketHeader(int userId, MessageKind kind, uint sequenceNumber)
            {
                UserId = userId;
                Kind = kind;
                SequenceNumber = sequenceNumber;
            }
        }
             
        public class ChatPacket
        {
            //Json.NET bug?
            public int DestinationId { get; set; }
            public string ChatContent { get; }

            public ChatPacket(int destId, string chatContent)
            {
                DestinationId = destId;
                ChatContent = chatContent;
            }
        }

        public class LogOnPacket
        {
            
        }

        public class LogOutPacket
        {
            
        }

        public class HeartBeatPacket
        {
           
        }
    }    
}
