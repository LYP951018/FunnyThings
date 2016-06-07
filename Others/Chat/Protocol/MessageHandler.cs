using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Protocol
{
    public static class MessageHandler
    {
        public static async void SendMessage(TcpClient client, object header, object body)
        {
            var headerJson = JsonConvert.SerializeObject(header);
            var bodyJson = JsonConvert.SerializeObject(body);
            var builder = new StringBuilder(headerJson.Length + bodyJson.Length + 10);
            var stringWriter = new StringWriter(builder);
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("Header");
                jsonWriter.WriteValue(headerJson);
                jsonWriter.WritePropertyName("Body");
                jsonWriter.WriteValue(bodyJson);
                jsonWriter.WriteEndObject();
            }
            var bytesToSend = Encoding.ASCII.GetBytes(builder.ToString());
            var lengthBytes = BitConverter.GetBytes(bytesToSend.Length);
            var ns = client.GetStream();
            await ns.WriteAsync(lengthBytes, 0, lengthBytes.Length);
            await ns.WriteAsync(bytesToSend, 0, bytesToSend.Length);
        }

        struct Packet
        {
            public string Header { get; set; }
            public string Body { get; set; }
        }

        public static async void ReceiveMessage(TcpClient client, Action<string, string, TcpClient> action)
        {
            var buffer = new byte[2048];
            var chars = new char[2048];
            var builder = new StringBuilder();
            var decoder = Encoding.ASCII.GetDecoder();
            var ns = client.GetStream();
            while (true)
            {
                builder.Clear();
                if (!client.Connected)
                    break;

                //先得到总长度。
                await ns.ReadAsync(buffer, 0, 4);
                var totalLength = BitConverter.ToInt32(buffer, 0);
                var bytesToRecv = totalLength;

                do
                {
                    var bytesRead = await ns.ReadAsync(buffer, 0, Math.Min(bytesToRecv, buffer.Length));
                    var charsCount = decoder.GetChars(buffer, 0, bytesRead, chars, 0);
                    builder.Append(chars, 0, charsCount);
                    bytesToRecv -= bytesRead;
                } while (bytesToRecv > 0);

                var json = builder.ToString();

                var packet = JsonConvert.DeserializeObject<Packet>(json);

                action(packet.Header, packet.Body, client);
            }            
        }
    }
}