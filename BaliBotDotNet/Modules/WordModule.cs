using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Models;
using BaliBotDotNet.Utilities.ExtensionMethods;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RunMode = Discord.Interactions.RunMode;

namespace BaliBotDotNet.Modules
{
    public class WordModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IAuthorRepository _authorRepository;
        public WordModule(IMessageRepository messageRepository, IAuthorRepository authorRepository)
        {
            _messageRepository = messageRepository;
            _authorRepository = authorRepository;
        }

        [SlashCommand("leaderboard", "Gets the leaderboard of most active users")]
        public async Task LeaderboardAsync(int maximum = 10)
        {
            await DeferAsync();
            ulong guildID = Context.Guild.Id;
            if (maximum > 20 || maximum < 1)
            {
                await FollowupAsync("Maximum must be between 1 and 20");
                return;
            }
            var leaderboard = _messageRepository.GetLeaderboard(guildID, maximum);
            await FollowupAsync(leaderboard.Select((kvPair, i) => $"#{i + 1} {kvPair.Key} {kvPair.Value}").Join('\n'));
        }

        [SlashCommand("socialcredit", "Displays social credit")]
        public async Task SocialCredit()
        {
            Random rng = new();
            int credit = rng.Next(0, 500);
            await RespondAsync($"You have {credit} social credits");
        }

        [SlashCommand("messagecount", "Gets the message count of the user who calls the command")]
        public async Task MessageCountAsync()
        {
            await DeferAsync();
            ulong authorID = Context.User.Id;
            var leaderboard = _messageRepository.GetAllMessages(Context.Guild.Id, authorID);
            await FollowupAsync($"You sent {leaderboard.Count} messages.");
        }

        [SlashCommand("reload", "Loads message history", runMode: RunMode.Async)]
        public async Task ReloadAsync(bool reloadSingleChannel = false)
        {
            if (!Context.User.Username.Equals("Bali"))
            {
                await RespondAsync($"{MentionUtils.MentionUser(Context.User.Id)} you can't use that!");
                return;
            }

            await RespondAsync("Loading....");
            const int messageCount = 10_000_000;
            var channels = Context.Guild.TextChannels;
            int numberOfProcessedMessages = 0;

            if (reloadSingleChannel)
            {
                _messageRepository.DropMessages(Context.Channel.Id);
            }
            else
            {
                _messageRepository.DropMessages(Context.Guild.Id);
            }

            foreach (var channel in channels)
            {
                if(reloadSingleChannel && channel.Id != Context.Channel.Id)
                {
                    continue;
                }
                IEnumerable<IMessage> messages = null;
                try
                {
                    messages = await channel.GetMessagesAsync(messageCount).FlattenAsync();
                    messages = messages.Where(x => !x.Author.IsBot && !x.ToString().StartsWith('$') && !x.ToString().StartsWith("p!c"));
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

        [SlashCommand("wordlength", "Finds the most used word with the specified length", runMode: RunMode.Async)]
        public async Task WordLengthAsync(int wordLength = 1)
        {
            await DeferAsync();
            if (wordLength <= 0)
            {
                await FollowupAsync("You must specify a minimum length greater than 0.");
                return;
            }

            if (Context.User.Username.Equals("Luneth"))
            {
                await FollowupAsync($"{MentionUtils.MentionUser(Context.User.Id)} you can't use that!");
                return;
            }

            var dict = LoadMessages(wordLength);
            var kv = dict.FirstOrDefault(x => x.Value == dict.Values.Max());
            if (string.IsNullOrEmpty(kv.Key))
            {
                await FollowupAsync($"There are no words that are {wordLength} letters long.");
            }
            else
            {
                if (MentionUtils.TryParseUser(kv.Key, out ulong mention))
                {
                    await FollowupAsync("This would ping someone :(");
                }
                else
                {
                    await FollowupAsync($"The most common word with {wordLength} letters is \"{kv.Key}\" with {kv.Value} occurences.");
                }
            }
        }
        [SlashCommand("choose", "Picks something in a list.")]
        public async Task Choose(string choicesString)
        {
            string[] choices = choicesString.Split(' ');
            Random rng = new();
            if (rng.Next(0, 1000) == 420)
            {
                await RespondAsync("lol cringe");
                return;
            }
            int n = choices.Length;
            if (n == 0)
            {
                await RespondAsync("You must specify at least one thing.");
                return;
            }
            int pick = rng.Next(0, n);
            await RespondAsync(choices[pick]);
        }

        [SlashCommand("coinflip", "Heads or tails")]
        public async Task CoinFlip()
        {
            Random rng = new();
            string answer = rng.Next(0, 2) % 2 == 0 ? "heads" : "tails";
            await RespondAsync($"{answer}");
        }

        [SlashCommand("quote", "Quotes someone at random, without context",runMode:RunMode.Async)]
        public async Task Quote()
        {
            await DeferAsync();
            var messageList = _messageRepository.GetAllMessages(Context.Guild.Id);
            var rng = new Random();
            var index = rng.Next(messageList.Count);
            var message = messageList[index];
            while (message.Content.Contains('@') || message.Content.Equals(""))
            {
                Console.WriteLine($"Tried to send {message.Content}");
                index = rng.Next(messageList.Count);
                message = messageList[index];
            }
            var author = _authorRepository.GetAuthor(message.AuthorID);
            await FollowupAsync($"{message.Content} -{author.Username}, {message.DateSent:dd MMMM yyyy}");
            //await Context.Channel.SendMessageAsync($"{message.Content} -{author.Username}, {message.DateSent:dd MMMM yyyy}");
        }

        [SlashCommand("count", "Counts the number of occurences of a specified word")]
        public async Task WordCountAsync(string word)
        {
            await DeferAsync();
            if (word.IsNullOrEmpty())
            {
                await FollowupAsync("You must specify a word to search.");
            }
            var dict = LoadMessages();
            if (dict.TryGetValue(word, out int count))
            {
                await FollowupAsync($"The word \"{word}\" has been used {count} time(s).");
            }
            else
            {
                await FollowupAsync("That word was never used in this server.");
            }
        }
        [SlashCommand("ngram", "Returns a list of n-grams by the user", runMode: RunMode.Async)]
        public async Task DisplayNGrams(int size = 0)
        {
            await DeferAsync();
            List<string> results = new();
            var messages = _messageRepository.GetAllMessages(Context.Guild.Id, Context.User.Id);

            if (size != 0)
            {
                results.Add(FetchNGram(size, messages));
            }
            else
            {
                for (int i = 2; i <= 6; i++)
                {
                    results.Add($"{i}-grams");
                    results.Add(FetchNGram(i, messages));
                    results.Add("\n");
                }
            }
            await FollowupAsync(results.Join('\n'));
        }

        private string FetchNGram(int size, List<Message> messages)
        {
            messages = messages.Where(x => x.Content.Split(" ").Length >= size).ToList();
            Dictionary<string, int> dict = new();
            foreach (var message in messages)
            {
                var msg = message.Content;
                var res = ngrams(size, msg);
                foreach (var gram in res)
                {
                    var cleanGram = Regex.Replace(gram, @"\p{Cs}", "");
                    if (dict.ContainsKey(cleanGram))
                    {
                        dict[cleanGram]++;
                    }
                    else
                    {
                        dict[cleanGram] = 1;
                    }
                }
            }
            dict = dict.Where(x => x.Value >= 5
                            && !string.IsNullOrWhiteSpace(x.Key)
                            && x.Key.Trim().Length > 1
                            && !x.Key.Contains('|')
                            && !x.Key.Contains('<'))
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToDictionary(dict => dict.Key, dict => dict.Value);
            return dict.Select((kvPair, i) => $"{kvPair.Value} {kvPair.Key}").Join('\n');
        }

        private List<string> ngrams(int n, string str)
        {
            List<string> ngrams = new List<string>();
            string[] words = str.Split(" ");
            for (int i = 0; i < words.Length - n + 1; i++)
                ngrams.Add(Concat(words, i, i + n));
            return ngrams;
        }

        private string Concat(string[] words, int start, int end)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = start; i < end; i++)
                sb.Append((i > start ? " " : "") + words[i]);
            return sb.ToString();
        }

        private Dictionary<string, int> LoadMessages(int wordLength = 0)
        {
            var messages = _messageRepository.GetAllMessages(Context.Guild.Id);
            Dictionary<string, int> dict = new();
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
