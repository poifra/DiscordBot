using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Utilities.ExtensionMethods;
using Discord;
using Discord.Commands;
using System;
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

        [Command("messagecount")]
        [Summary("Gets the message count of the user who calls the command")]
        public async Task MessageCountAsync()
        {
            ulong authorID = Context.User.Id;
            var leaderboard = _messageRepository.GetAllMessages(Context.Guild.Id, authorID);
            await ReplyAsync($"You sent {leaderboard.Count} messages.");
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

            _messageRepository.DropMessages(Context.Guild.Id);

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

        [Command("wordlength", RunMode = RunMode.Async)]
        [Summary("Finds the most used word with the specified length")]
        public async Task WordLengthAsync(int wordLength = 1)
        {
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

            var dict = LoadMessages(wordLength);
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
        [Command("choose")]
        [Summary("Picks one")]
        public async Task Choose(params string[] choices)
        {
            Random rng = new Random();
            Random cringeRNG = new Random();
            if (cringeRNG.Next(0, 1000) == 420)
            {
                await ReplyAsync("lol cringe");
                return;
            }
            int n = choices.Length;
            if (n == 0)
            {
                await ReplyAsync("You must specify at least one thing.");
                return;
            }
            int pick = rng.Next(0, n);
            await ReplyAsync(choices[pick]);
        }

        [Command("coinflip")]
        [Summary("Heads or tails")]
        public async Task CoinFlip()
        {
            Random rng = new();
            string answer = rng.Next(0, 2) % 2 == 0 ? "heads" : "tails";
            await ReplyAsync($"{answer}");
        }

        [Command("count")]
        [Summary("Counts the number of occurences of a specified word")]
        public async Task WordCountAsync(string word)
        {
            if (word.IsNullOrEmpty())
            {
                await ReplyAsync("You must specify a word to search.");
            }
            var dict = LoadMessages();
            if (dict.TryGetValue(word, out int count))
            {
                await ReplyAsync($"The word \"{word}\" has been used {count} time(s).");
            }
            else
            {
                await ReplyAsync("That word was never used in this server.");
            }
        }

        private Dictionary<string, int> LoadMessages(int wordLength = 0)
        {
            var messages = _messageRepository.GetAllMessages(Context.Guild.Id);
            Dictionary<string, int> dict = new Dictionary<string, int>();
            foreach (var m in messages)
            {
                IEnumerable<string> words = m.Content.Split(' ').ToList();
                if (wordLength != 0)
                {
                    words = words.Where(x => x.Length == wordLength);
                }
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
            return dict;
        }
    }
}
