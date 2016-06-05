using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class MessageHandler
    {
        private ulong _sequenceNumber;
        private bool _isClient;

        public MessageHandler(bool isClient)
        {
            _isClient = isClient;
            _sequenceNumber = isClient ? 1ul : 2ul;
        }

        //TODO：使用继承
        public async Task<ulong> SendMessage(IPAddress address, Client.Packet message)
        {
            var prevSequenceNumber = _sequenceNumber;
            message.SequenceNumber = prevSequenceNumber;
            //TODO: 这里需要同步吗？
            ++_sequenceNumber;
            await SendMessageInternal(address, message);
            return prevSequenceNumber;
        }

        public async Task SendMessage(IPAddress address, Server.Packet message)
        {
            var prevSequenceNumber = _sequenceNumber;
            message.SequenceNumber = prevSequenceNumber + 1;
            ++_sequenceNumber;
            await SendMessageInternal(address, message);
        }

        private async Task SendMessageInternal(IPAddress address, object message)
        {
            using (var client = new TcpClient())
            {
                client.ConnectAsync(address, _isClient ? Config.ServerPortNumber : Config.ClientPortNumber).Wait();
                var bytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(message));
                var lengthBytes = BitConverter.GetBytes(bytes.Length);
                Debug.Assert(lengthBytes.Length == sizeof(int));
                var ns = client.GetStream();
                await ns.WriteAsync(lengthBytes, 0, sizeof(int));
                await ns.WriteAsync(bytes, 0, bytes.Length);
            }
        }
    }
}
