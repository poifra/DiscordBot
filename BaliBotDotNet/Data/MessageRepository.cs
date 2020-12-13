using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Models;
using Dapper;
using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace BaliBotDotNet.Model
{
    public class MessageRepository : SqlLiteBaseRepository, IMessageRepository
    {
        public MessageRepository() : base()
        {

        }

        public List<Message> GetAllMessages(ulong guildID, ulong authorID = 0)
        {
            IEnumerable<Message> messageList;
            using var con = SqlLiteConnexion();
            var sql = "SELECT * FROM Message WHERE GuildID=@GuildID ";
            if (authorID != 0)
            {
                sql += "AND AuthorID=@AuthorID ";
            }
            var parameters = new
            {
                AuthorID = authorID,
                GuildID = guildID
            };
            con.Open();
            messageList = con.Query<Message>(sql, parameters);
            return messageList.AsList();
        }

        public void InsertBulkMessage(IEnumerable<IMessage> messages, SocketGuild guild)
        {
            using var con = SqlLiteConnexion();
            con.Open();
            var transation = con.BeginTransaction();
            foreach (var message in messages)
            {
                InsertMessage(message, guild, con);
            }
            transation.Commit();
        }

        public void InsertMessage(IMessage discordMessage, SocketGuild guild, SqliteConnection con = null)
        {
            if(con == null)
            {
                con = SqlLiteConnexion();
            }
            var sqlMessage = "INSERT OR IGNORE INTO Message (MessageID, AuthorID, GuildID, Content) VALUES (@MessageID, @AuthorID, @GuildID, @Content)";
            var sqlAuthor = "INSERT OR IGNORE INTO Author (AuthorID, Username, DiscordID) VALUES (@AuthorID, @Username, @DiscordID)";
            var messageParameters = new
            {
                MessageID = discordMessage.Id,
                AuthorID = discordMessage.Author.Id,
                GuildID = guild.Id,
                Content = discordMessage.Content
            };

            string[] userInfo = discordMessage.Author.ToString().Split('#');
            var authorParameters = new
            {
                AuthorID = discordMessage.Author.Id,
                Username = userInfo[0],
                DiscordID = int.Parse(userInfo[1])

            };
            if (con.State != System.Data.ConnectionState.Open)
            {
                con.Open();
            }
            con.Execute(sqlAuthor, authorParameters);
            con.Execute(sqlMessage, messageParameters);
        }
    }
}
