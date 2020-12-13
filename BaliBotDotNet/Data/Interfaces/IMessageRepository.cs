using BaliBotDotNet.Models;
using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaliBotDotNet.Data.Interfaces
{
    public interface IMessageRepository
    {
        void InsertMessage(IMessage message, SocketGuild guild, SqliteConnection con = null);
        void InsertBulkMessage(IEnumerable<IMessage> messages, SocketGuild guild);
        List<Message> GetAllMessages(ulong guildID, ulong authorID = 0);
     
    }
}
