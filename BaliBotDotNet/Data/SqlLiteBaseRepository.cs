using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace BaliBotDotNet.Data
{
    public class SqlLiteBaseRepository
    {
        private static SqliteConnection _connection = null;
        public static SqliteConnection SqlCon
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new SqliteConnection("Data Source=" + DbFile);
                }
                return _connection;
            }
        }
        public static string DbFile
        {
            get { return Environment.CurrentDirectory + "\\BaliBotDB.sqlite"; }
        }

        internal SqlLiteBaseRepository()
        {
            if (!File.Exists(DbFile))
            {
                CreateDatabase();
            }
        }
        private static void CreateDatabase()
        {
            using var con = SqlCon;
            con.Open();
            con.Execute(@"
                    create table Author(
                    AuthorID integer primary key,
                    Username text not null,
                    DiscordID integer not null)");

            con.Execute(@"
                    create table Message(                    
                    MessageID integer primary key,
                    AuthorID integer not null,
                    GuildID integer not null,
                    Content text not null,
                    foreign key(AuthorID) references Author(AuthorID));");

            con.Execute(@"
                   create table Reminder(
                   ReminderID integer primary key,
                   AuthorID integer not null,
                   ChannelID integer not null,
                   ReminderText text not null,
                   ReminderTime text not null,
                   IsReminderDone integer not null default 0);
            ");
        }
    }
}
