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
       
        public struct PacketBase
        {
            public ulong SequenceNumber { get; set; }
            public MessageKind Kind { get; set; }
        }

        public struct Packet
        {
            private PacketBase _base;

            public PacketBase Header => _base;

            public ulong SequenceNumber
            {
                get { return _base.SequenceNumber; }
                set { _base.SequenceNumber = value; }
            }

            public MessageKind Kind
            {
                get { return _base.Kind; }
                set { _base.Kind = value; }
            }

            public object Body { get; set; }
        }


        public struct PayloadPacket<T>
        {
            public PacketBase Header { get; set; }
            private T _body;

            public object UserData { get; set; }

            public PayloadPacket(Packet packet, object userData = null)
            {
                Header = packet.Header;
                _body = (T)packet.Body;
                UserData = userData;
            }

            public T Body => _body;
        }

        public struct ChatBody
        {
            public int UserId { get; set; }
            public int DestinationId { get; set; }
            public string ChatContent { get; set; }
        }

        public struct LogOnBody
        {
            public int UserId { get; set; }
        }

        public struct LogOutBody
        {
            public int UserId { get; set; }
        }

        public struct HeartBeatBody
        {
            public int UserId { get; set; }
        }
    }    
}
