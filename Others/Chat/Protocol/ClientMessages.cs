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
       
        public struct PacketHeader
        {
            public int UserId { get; set; }
            public MessageKind Kind { get; set; }            
        }
             
        public struct ChatPacket
        {           
            public int DestinationId { get; set; }
            public string ChatContent { get; set; }
        }

        public struct LogOnPacket
        {
            
        }

        public struct LogOutPacket
        {
            
        }

        public struct HeartBeatPacket
        {
           
        }
    }    
}
