using System;

namespace BaliBotDotNet.Models
{
    public class Reminder
    {
        public int ReminderID { get; set; }
        public ulong AuthorID { get; set; }
        public ulong ChannelID { get; set; }
        public DateTime ReminderTime { get; set; }
        public string ReminderText { get; set; }
        public bool IsReminderDone { get; set; }
    }
}
