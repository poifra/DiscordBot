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

namespace BaliBotDotNet.Data
{
    public class MessageRepository(BaliBotDbContext dbContext) : IMessageRepository
    {
        private readonly BaliBotDbContext _db = dbContext;

        public List<LeaderboardGrouping> GetLeaderboard(ulong guildID, int maximum = 10)
        {
            var rs = _db.Messages
                    .Where(x => x.GuildID == guildID)
                    .GroupBy(x => x.Author)
                    .Select(group => new LeaderboardGrouping
                    { 
                            User = group.Key.Username, 
                            Count = group.Count() 
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(maximum)
                    .ToList();
            return rs;

        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "<Pending>")]
        public List<Message> GetAllMessages(ulong guildID, ulong authorID = 0)
        {
            var rs = _db.Messages.Where(x => x.GuildID == guildID);
            if (authorID != 0)
            {
                rs = rs.Where(x => x.AuthorID == authorID);
            }
            return rs.ToList();
        }

        public void InsertBulkMessage(IEnumerable<IMessage> messages, SocketGuild guild)
        {
            foreach (IMessage message in messages)
            {
                InsertMessage(message, guild);
            }
        }

        public void InsertMessage(IMessage discordMessage, SocketGuild guild, SqliteConnection con = null)
        {
            var author = _db.Authors.FirstOrDefault(x => x.AuthorID == discordMessage.Author.Id);
            if (author == null)
            {
                _db.Authors.Add(new Author
                {
                    AuthorID = discordMessage.Author.Id,
                    Username = discordMessage.Author.ToString(),
                });
            }
            _db.Messages.Add(new Message
            {
                MessageID = discordMessage.Id,
                AuthorID = discordMessage.Author.Id,
                GuildID = guild.Id,
                Content = discordMessage.Content,
                DateSent = discordMessage.Timestamp.DateTime.ToString(CultureInfo.InvariantCulture)
            });

            _db.SaveChanges();
        }

        public void DropMessages(ulong serverID)
        {
            var messageList = _db.Messages.Where(x => x.GuildID == serverID);
            foreach (var message in messageList)
            {
                _db.Messages.Remove(message);
            }
            _db.SaveChanges();
        }
    }

    public class LeaderboardGrouping
    {
        public string User { get; internal set; }
        public int Count { get; internal set; }
    }
}
