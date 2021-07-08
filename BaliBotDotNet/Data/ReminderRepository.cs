using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Models;
using Dapper;
using System;
using System.Collections.Generic;

namespace BaliBotDotNet.Data
{
    public class ReminderRepository : SqlLiteBaseRepository, IReminderRepository
    {
        public ReminderRepository() : base()
        { }

        public List<Reminder> CheckForReminders()
        {
            using var con = SqlCon;
            con.Open();
            var sql = "SELECT * FROM Reminder where IsReminderDone=0 AND ReminderTime<DATETIME('now','localtime');";
            var reminders = con.Query<Reminder>(sql);
            return reminders.AsList();
        }

        public void InsertReminder(ulong authorID, ulong channelID, DateTime reminderDate, string reminderText)
        {
            using var con = SqlCon;
            con.Open();
            var sqlInsert = "INSERT INTO Reminder (AuthorID, ChannelID, ReminderText, ReminderTime) VALUES (@AuthorID, @ChannelID, @ReminderText, @ReminderTime);";
            var reminderParameters = new
            {
                AuthorID = authorID,
                ChannelID = channelID,
                ReminderText = reminderText,
                ReminderTime = reminderDate.ToString()
            };
            con.Execute(sqlInsert, reminderParameters);
        }

        public void SetReminderDone(int reminderID)
        {
            using var con = SqlCon;
            con.Open();
            var sqlUpdate = "UPDATE Reminder SET IsReminderDone = 1 WHERE ReminderID=@ReminderID;";
            var updateParameters = new
            {
                ReminderID = reminderID,
            };
            con.Execute(sqlUpdate, updateParameters);

        }
    }
}
