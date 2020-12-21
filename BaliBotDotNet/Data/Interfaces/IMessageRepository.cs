using BaliBotDotNet.Models;
using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace BaliBotDotNet.Data.Interfaces
{
    public interface IMessageRepository
    {
        void InsertMessage(IMessage message, SocketGuild guild, SqliteConnection con = null);
        void InsertBulkMessage(IEnumerable<IMessage> messages, SocketGuild guild);
        List<Message> GetAllMessages(ulong guildID, ulong authorID = 0);
        Dictionary<string, int> GetLeaderboard(ulong guildID, int maximum = 10);


    }
}
