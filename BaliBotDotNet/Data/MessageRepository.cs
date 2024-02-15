﻿using BaliBotDotNet.Data;
using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Models;
using Dapper;
using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BaliBotDotNet.Model
{
    public class MessageRepository : SqlLiteBaseRepository, IMessageRepository
    {
        public MessageRepository() : base()
        {

        }

        public Dictionary<string, int> GetLeaderboard(ulong guildID, int maximum = 10)
        {
            var sql = "select count(*) as nb, a.Username from Message m " +
                "inner join Author a on a.AuthorID = m.AuthorID " +
                "where m.GuildID=@guildID " +
                "group by m.AuthorID " +
                "order by nb desc " +
                "limit @maximum";
            var con = SqlCon;
            if (con.State != System.Data.ConnectionState.Open)
            {
                con.Open();
            }
            if (maximum > 20)
            {
                return new Dictionary<string, int>();
            }
            var parameters = new { guildID, maximum };
            var result = con.Query(sql, parameters).ToDictionary(row => (string)row.Username, row => (int)row.nb);
            return result;

        }
        public List<Message> GetAllMessages(ulong guildID, ulong authorID = 0)
        {
            IEnumerable<Message> messageList;
            var con = SqlCon;
            if (con.State != System.Data.ConnectionState.Open)
            {
                con.Open();
            }
            var sql = "SELECT * FROM Message M inner join Author A on A.AuthorID=M.AuthorID WHERE M.GuildID=@GuildID AND A.IsQuotable=1";
            if (authorID != 0)
            {
                sql += " AND M.AuthorID=@AuthorID ";
            }
            var parameters = new
            {
                AuthorID = authorID,
                GuildID = guildID
            };
            messageList = con.Query<Message>(sql, parameters);
            return messageList.AsList();
        }

        public void InsertBulkMessage(IEnumerable<IMessage> messages, SocketGuild guild)
        {
            var con = SqlCon;

            if (con.State != System.Data.ConnectionState.Open)
            {
                con.Open();
            }
            var transation = con.BeginTransaction();
            foreach (var message in messages)
            {
                InsertMessage(message, guild, con);
            }
            transation.Commit();
        }

        public void InsertMessage(IMessage discordMessage, SocketGuild guild, SqliteConnection con = null)
        {
            if (con == null)
            {
                con = SqlCon;
            }
            var sqlMessage = "INSERT OR IGNORE INTO Message (MessageID, AuthorID, GuildID, Content, DateSent) VALUES (@MessageID, @AuthorID, @GuildID, @Content, @Date)";
            var sqlAuthor = "INSERT OR IGNORE INTO Author (AuthorID, Username) VALUES (@AuthorID, @Username)";
            var messageParameters = new
            {
                MessageID = discordMessage.Id,
                AuthorID = discordMessage.Author.Id,
                GuildID = guild.Id,
                discordMessage.Content,
                Date = discordMessage.Timestamp.DateTime.ToString(CultureInfo.InvariantCulture),
            };

            string userInfo = discordMessage.Author.ToString();
            var authorParameters = new
            {
                AuthorID = discordMessage.Author.Id,
                Username = userInfo

            };
            if (con.State != System.Data.ConnectionState.Open)
            {
                con.Open();
            }
            con.Execute(sqlAuthor, authorParameters);
            con.Execute(sqlMessage, messageParameters);
        }

        public void DropMessages(ulong serverID)
        {
            var con = SqlCon;
            if (con.State != System.Data.ConnectionState.Open)
            {
                con.Open();
            }
            var sql = "DELETE FROM Message WHERE GuildID=@GuildID";
            var sqlParams = new { GuildID = serverID };
            con.Execute(sql, sqlParams);
        }
    }
}
