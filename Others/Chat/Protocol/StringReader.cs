using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public static class StringReader
    {
        public static async Task<string> Read(NetworkStream ns, int bufferSize = 1024)
        {
            var sb = new StringBuilder();
            var buffer = new byte[bufferSize];
            await ns.ReadAsync(buffer, 0, sizeof(int));
            var bytesToRecv = BitConverter.ToInt32(buffer, 0);
            var decoder = Encoding.ASCII.GetDecoder();
            do
            {
                var bytesRead = await ns.ReadAsync(buffer, 0, buffer.Length);
                bytesToRecv -= bytesRead;
                var charLength = decoder.GetCharCount(buffer, 0, bytesRead);
                var chars = new char[charLength];
                decoder.GetChars(buffer, 0, bytesRead, chars, 0);
                sb.Append(chars);
            } while (bytesToRecv > 0);

            return sb.ToString();
        }
    }
}
