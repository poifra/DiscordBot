using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace BaliBotDotNet.Model
{
    public class SqlLiteBaseRepository
    {
        public static string DbFile
        {
            get { return Environment.CurrentDirectory + "\\BaliBotDB.sqlite"; }
        }

        public static SqliteConnection SqlLiteConnexion()
        {
            return new SqliteConnection("Data Source=" + DbFile);
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
            using var con = SqlLiteConnexion();
            con.Open();
            con.Execute(@"
                    create table Author(
                    AuthorID integer identity primary key autoincrement,
                    Username varchar(max),
                    DiscordID)");

            con.Execute(@"
                    create table Message(                    
                    MessageID integer identity primary key autoincrement,
                    AuthorID integer not null,
                    GuildID integer not null,
                    Content varchar(max) not null
                    foreign key(AuthorID) references Author(AuthorID));");
        }
    }
}
