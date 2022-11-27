using System;

namespace BaliBotDotNet.Models
{
    public class Message
    {
        public ulong MessageID { get; set; }
        public ulong AuthorID { get; set; }
        public ulong GuildID { get; set; }
        public string Content { get; set; }
        public DateTime DateSent { get; set; }
    }
}
