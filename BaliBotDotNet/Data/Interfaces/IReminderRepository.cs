using BaliBotDotNet.Models;
using System;
using System.Collections.Generic;

namespace BaliBotDotNet.Data.Interfaces
{
    public interface IReminderRepository
    {
        List<Reminder> CheckForReminders();
        int InsertReminder(ulong authorID, ulong channelID, DateTime reminderDate, string reminderText);
        void SetReminderDone(int reminderID);
        void DeleteReminder(int reminderID);
        Reminder GetReminder(int reminderID);
    }
}
