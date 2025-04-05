using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaliBotDotNet.Data
{
    public class ReminderRepository : IReminderRepository
    {
        private readonly BaliBotDbContext _db;
        public ReminderRepository(BaliBotDbContext dbContext)
        { 
            _db = dbContext;
        }

        public List<Reminder> CheckForReminders()
        {
            var rs = _db.Reminders.Where(x => x.IsReminderDone == 0 && x.ReminderTime < DateTime.Now);
            return rs.ToList();
        }

        public void DeleteReminder(int reminderID)
        {
            var reminder = _db.Reminders.FirstOrDefault(x => x.ReminderID == reminderID);
            if (reminder != null)
            {
                _db.Reminders.Remove(reminder);
            }
        }

        public Reminder GetReminder(int reminderID)
        {
            var reminder = _db.Reminders.FirstOrDefault(x => x.ReminderID == reminderID);
            return reminder;
        }

        public int InsertReminder(ulong authorID, ulong channelID, DateTime reminderDate, string reminderText)
        {
            var reminder = new Reminder
            {
                AuthorID = authorID,
                ChannelID = channelID,
                ReminderText = reminderText,
                ReminderTime = reminderDate,
            };
            _db.Reminders.Add(reminder);
               
            _db.SaveChanges();
            return reminder.ReminderID;
        }

        public void SetReminderDone(int reminderID)
        {
            var reminder = _db.Reminders.FirstOrDefault(x => x.ReminderID == reminderID);
            if (reminder != null)
            {
                reminder.IsReminderDone = 1;
            }
            _db.SaveChanges();

        }
    }
}
