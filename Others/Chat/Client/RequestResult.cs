using Protocol;
using Protocol.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class LogOnRequestResult
    {
        public ErrorKind Error { get; }
        public UserInfo Info { get; }
        public List<int> OnlineUsers { get; }

        public LogOnRequestResult(ErrorKind kind, UserInfo info, IEnumerable<int> onlineUsers)
        {
            Error = kind;
            Info = info;
            OnlineUsers = onlineUsers == null ? null : new List<int>(onlineUsers);
        }
    }
}
