using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Models;
using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
        public List<Message> GetAllMessages(ulong guildID, ulong authorID = 0)
        {
            var rs = _db.Messages.AsNoTracking().Where(x => x.GuildID == guildID && x.Author.IsQuotable);
            if (authorID != 0)
            {
                rs = rs.Where(x => x.AuthorID == authorID);
            }
            return [.. rs];
        }

        public void InsertBulkMessage(IEnumerable<IMessage> messages, SocketGuild guild)
        {
            var newAuthors = messages
                .Select(x => x.Author)
                .DistinctBy(x => x.Id)
                .Where(a => !_db.Authors.Any(dbA => dbA.AuthorID == a.Id))
                .Select(a => new Author
                {
                    AuthorID = a.Id,
                    Username = a.ToString(),
                });
            _db.Authors.AddRange(newAuthors);
            _db.AddRange(messages
                .Where(m => !_db.Messages.Any(dbM => dbM.MessageID == m.Id))
                .Select(m => new Message
                {
                    MessageID = m.Id,
                    AuthorID = m.Author.Id,
                    GuildID = guild.Id,
                    Content = m.Content,
                    DateSent = m.Timestamp.DateTime.ToString(CultureInfo.InvariantCulture)
                }));
            _db.SaveChanges();
            _db.ChangeTracker.Clear();

        }

        public void InsertMessage(IMessage discordMessage, SocketGuild guild, SqliteConnection con = null)
        {
            var messageExists = _db.Messages.Any(x => x.MessageID == discordMessage.Id);
            var author = _db.Authors.FirstOrDefault(x => x.AuthorID == discordMessage.Author.Id);
            if (author == null)
            {
                author = new Author
                {
                    AuthorID = discordMessage.Author.Id,
                    Username = discordMessage.Author.ToString(),
                };
                _db.Authors.Add(author);
            }
            if (!messageExists)
            {
                _db.Messages.Add(new Message
                {
                    MessageID = discordMessage.Id,
                    AuthorID = discordMessage.Author.Id,
                    GuildID = guild.Id,
                    Content = discordMessage.Content,
                    DateSent = discordMessage.Timestamp.DateTime.ToString(CultureInfo.InvariantCulture)
                });
            }
            _db.SaveChanges();
        }

        public void DropMessages(ulong serverID)
        {
            var messageList = _db.Messages.Where(x => x.GuildID == serverID);
            _db.Messages.RemoveRange(messageList);
            _db.SaveChanges();
        }

        public Message GetMostRecentMessage(ulong guildId)
        {
            return _db.Messages.AsNoTracking().Where(x => x.GuildID == guildId).OrderByDescending(x => x.DateSent).FirstOrDefault();
        }

        public void DeleteMessage(ulong messageId)
        {
            var message = _db.Messages.FirstOrDefault(x => x.MessageID == messageId);
            if (message != null)
            {
                _db.Messages.Remove(message);
                _db.SaveChanges();
            }
        }
    }

    public class LeaderboardGrouping
    {
        public string User { get; internal set; }
        public int Count { get; internal set; }
    }
}
