using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Protocol
{
    public struct UserInfo
    {
        /// <summary>
        /// 每个用户独一无二的 ID。LogOn、LogOut、Chat、HeartBeat 均需要。
        /// </summary>
        public int UserID { get; set; }
        /// <summary>
        /// 只有 LogOn 需要。
        /// </summary>
        public string UserName { get; set; }
        //public string EncryptedPassword { get; set; }
    }
}
