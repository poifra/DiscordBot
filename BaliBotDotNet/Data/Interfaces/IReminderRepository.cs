using BaliBotDotNet.Models;
using System;
using System.Collections.Generic;

namespace BaliBotDotNet.Data.Interfaces
{
    public interface IReminderRepository
    {
        List<Reminder> CheckForReminders();
        void InsertReminder(ulong authorID, ulong channelID, DateTime reminderDate, string reminderText);
        void SetReminderDone(int reminderID);
    }
}
