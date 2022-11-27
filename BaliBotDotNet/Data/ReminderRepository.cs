using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaliBotDotNet.Data
{
    public class ReminderRepository : SqlLiteBaseRepository, IReminderRepository
    {
        public ReminderRepository() : base()
        { }

        public List<Reminder> CheckForReminders()
        {
            var con = SqlCon;
            if (con.State != System.Data.ConnectionState.Open)
            {
                con.Open();
            }
            var sql = "SELECT * FROM Reminder where IsReminderDone=0 AND ReminderTime<DATETIME('now','localtime');";
            var reminders = con.Query<Reminder>(sql);
            return reminders.AsList();
        }

        public void DeleteReminder(int reminderID)
        {
            var con = SqlCon;
            if (con.State != System.Data.ConnectionState.Open)
            {
                con.Open();
            }
            var sqlUpdate = "DELETE FROM Reminder WHERE ReminderID=@ReminderID;";
            var updateParameters = new
            {
                ReminderID = reminderID,
            };
            con.Execute(sqlUpdate, updateParameters);
        }

        public Reminder GetReminder(int reminderID)
        {
            var con = SqlCon;
            if (con.State != System.Data.ConnectionState.Open)
            {
                con.Open();
            }
            var sqlUpdate = "SELECT * FROM Reminder WHERE ReminderID=@ReminderID;";
            var updateParameters = new
            {
                ReminderID = reminderID,
            };
            var reminder = con.Query<Reminder>(sqlUpdate, updateParameters).FirstOrDefault();
            return reminder;
        }

        public int InsertReminder(ulong authorID, ulong channelID, DateTime reminderDate, string reminderText)
        {
            var con = SqlCon;
            if (con.State != System.Data.ConnectionState.Open)
            {
                con.Open();
            }
            var sqlInsert = "INSERT INTO Reminder (AuthorID, ChannelID, ReminderText, ReminderTime) VALUES (@AuthorID, @ChannelID, @ReminderText, @ReminderTime);";
            var reminderParameters = new
            {
                AuthorID = authorID,
                ChannelID = channelID,
                ReminderText = reminderText,
                ReminderTime = reminderDate.ToString()
            };
            con.Execute(sqlInsert, reminderParameters);
            return con.QueryFirst<int>("SELECT last_insert_rowid();");
        }

        public void SetReminderDone(int reminderID)
        {
            var con = SqlCon;
            if (con.State != System.Data.ConnectionState.Open)
            {
                con.Open();
            }
            var sqlUpdate = "UPDATE Reminder SET IsReminderDone = 1 WHERE ReminderID=@ReminderID;";
            var updateParameters = new
            {
                ReminderID = reminderID,
            };
            con.Execute(sqlUpdate, updateParameters);

        }
    }
}
