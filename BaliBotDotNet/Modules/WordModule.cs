using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Utilities.ExtensionMethods;
using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaliBotDotNet.Modules
{
    public class WordModule : ModuleBase<SocketCommandContext>
    {
        private IMessageRepository _messageRepository;
        public WordModule(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        [Command("leaderboard")]
        [Summary("Gets the leaderboard of most active users")]
        public async Task LeaderboardAsync(int maximum = 10)
        {
            ulong guildID = Context.Guild.Id;
            if (maximum > 20 || maximum < 1)
            {
                await ReplyAsync("Maximum must be between 1 and 20");
            }
            var leaderboard = _messageRepository.GetLeaderboard(guildID, maximum);
            await ReplyAsync(leaderboard.Select((kvPair, i) => $"#{i + 1} {kvPair.Key} {kvPair.Value}").Join('\n'));
        }

        [Command("reload", RunMode = RunMode.Async)]
        public async Task ReloadAsync()
        {
            if (!Context.Message.Author.Username.Equals("Bali"))
            {
                await ReplyAsync($"{MentionUtils.MentionUser(Context.Message.Author.Id)} you can't use that!");
                return;
            }

            await ReplyAsync("Loading....");
            const int messageCount = 10_000_000;
            var channels = Context.Guild.TextChannels;
            int numberOfProcessedMessages = 0;

            foreach (var channel in channels)
            {
                IEnumerable<IMessage> messages = null;
                try
                {
                    messages = await channel.GetMessagesAsync(messageCount).FlattenAsync();
                    messages = messages.Where(x => !x.Author.IsBot && !x.ToString().StartsWith('$'));
                    _messageRepository.InsertBulkMessage(messages, Context.Guild);
                }
                catch (Discord.Net.HttpException)
                {
                    await ReplyAsync("I can't read " + channel.Name);
                }
                numberOfProcessedMessages += messages?.Count() ?? 0;
            }

            await ReplyAsync($"Done loading {numberOfProcessedMessages} messages!");
        }

        [Command("wordcount", RunMode = RunMode.Async)]
        [Summary("Test")]
        public async Task WordCountAsync(int wordLength = 1)
        {
            //   return;
            if (wordLength <= 0)
            {
                await ReplyAsync("You must specify a minimum length greater than 0.");
                return;
            }

            if (Context.Message.Author.Username.Equals("Luneth"))
            {
                await ReplyAsync($"{MentionUtils.MentionUser(Context.Message.Author.Id)} you can't use that!");
                return;
            }

            var messages = _messageRepository.GetAllMessages(Context.Guild.Id);
            Dictionary<string, int> dict = new Dictionary<string, int>();
            foreach (var m in messages)
            {
                IEnumerable<string> words = m.Content.Split(' ').ToList();
                words = words.Where(x => x.Length == wordLength);
                foreach (var w in words)
                {
                    if (dict.ContainsKey(w))
                    {
                        dict[w]++;
                    }
                    else
                    {
                        dict[w] = 1;
                    }
                }
            }
            var kv = dict.FirstOrDefault(x => x.Value == dict.Values.Max());
            if (string.IsNullOrEmpty(kv.Key))
            {
                await ReplyAsync($"There are no words that are {wordLength} letters long.");
            }
            else
            {
                if (MentionUtils.TryParseUser(kv.Key, out ulong mention))
                {
                    await ReplyAsync("This would ping someone :(");
                }
                else
                {
                    await ReplyAsync($"The most common word with {wordLength} letters is \"{kv.Key}\" with {kv.Value} occurences.");
                }

            }

        }
    }
}
