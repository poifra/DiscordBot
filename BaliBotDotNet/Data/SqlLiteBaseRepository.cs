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
                _connection ??= new SqliteConnection("Data Source=" + DbFile);
                return _connection;
            }
        }
        public static string DbFile
        {
            get { return Environment.CurrentDirectory + "\\BaliBotDB.sqlite"; }
        }

        internal SqlLiteBaseRepository()
        {
            ValidateDatabaseStructure();
        }
        private static void ValidateDatabaseStructure()
        {
            var con = SqlCon;
            if (con.State != System.Data.ConnectionState.Open)
            {
                con.Open();
            }
            con.Execute(@"
                    create table if not exists Author(
                    AuthorID integer primary key,
                    Username text not null,
                    IsQuotable bit not null default 1)");

            con.Execute(@"
                    create table if not exists Message(                    
                    MessageID integer primary key,
                    AuthorID integer not null,
                    GuildID integer not null,
                    Content text not null,
                    DateSent text not null,
                    foreign key(AuthorID) references Author(AuthorID));");

            con.Execute(@"
                    create table if not exists Reminder(
                    ReminderID integer primary key,
                    AuthorID integer not null,
                    ChannelID integer not null,
                    ReminderText text not null,
                    ReminderTime text not null,
                    IsReminderDone integer not null default 0);");

            con.Execute(@"create table if not exists AlternativeFact(
                    AlternativeFactId integer primary key,
                    Description text not null,
                    AuthorID integer not null,
                    foreign key(AuthorID) references Author(AuthorID));");
        }
    }
}
