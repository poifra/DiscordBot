using System;
using System.Collections.Generic;

namespace BaliBotDotNet.Models;

public class Reminder
{
    public int ReminderID { get; set; }

    public ulong AuthorID { get; set; }

    public ulong ChannelID { get; set; }

    public string ReminderText { get; set; }

    public DateTime ReminderTime { get; set; }

    public int IsReminderDone { get; set; }
}
