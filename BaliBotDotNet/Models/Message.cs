using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaliBotDotNet.Models
{
    public class Message
    {
        public ulong MessageID { get; set; }
        public ulong AuthorID { get; set; }
        public ulong GuildID { get; set; }
        public string Content { get; set; }
    }
}
