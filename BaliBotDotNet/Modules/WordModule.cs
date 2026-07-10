using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Models;
using BaliBotDotNet.Utilities.ExtensionMethods;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RunMode = Discord.Interactions.RunMode;

namespace BaliBotDotNet.Modules
{

    public class WordModule : InteractionModuleBase<SocketInteractionContext>
    {
        private sealed class LanguageModelCache
        {
            public DateTime BuiltAt { get; set; }
            public Dictionary<string, int> Unigrams { get; set; }
            public Dictionary<string, Dictionary<string, int>> Bigrams { get; set; }
            public int TotalUnigrams { get; set; }
        }

        private readonly IMessageRepository _messageRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IAlternativeFactRepository _alternativeFactRepository;
        private readonly IServiceScopeFactory _scopeFactory;
        // Simple per-guild language model cache to avoid rebuilding every invocation.
        private static readonly ConcurrentDictionary<ulong, LanguageModelCache> _languageModelCache = new();

        public WordModule(IMessageRepository messageRepository, 
                          IAuthorRepository authorRepository,
                          IAlternativeFactRepository alternativeFactRepository,
                          IServiceScopeFactory scopeFactory)
        {
            _messageRepository = messageRepository;
            _authorRepository = authorRepository;
            _alternativeFactRepository = alternativeFactRepository;
            _scopeFactory = scopeFactory;
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
            await FollowupAsync(leaderboard.Select((kvPair, i) => $"#{i + 1} {kvPair.User} {kvPair.Count}").Join('\n'));
        }

        [SlashCommand("averagesentence", "Gets the average of the server")]
        public async Task ServerAverage(int wordCount = 10)
        {
            await DeferAsync();

            if (wordCount < 3 || wordCount > 30)
            {
                await FollowupAsync("Count must be between 3 and 30 for a sensible sentence.");
                return;
            }

            var guildId = Context.Guild.Id;
            var model = GetOrBuildLanguageModel(guildId);

            if (model.Unigrams.Count == 0)
            {
                await FollowupAsync("Not enough data to build an average sentence.");
                return;
            }

            var rng = new Random();
            var sentence = GenerateAverageSentence(model, wordCount, rng);

            await FollowupAsync(sentence);
        }

        [SlashCommand("AverageHat", "Creates an average hat.")]
        public async Task AverageHat(int wordCount = 10)
        {
            await DeferAsync();

            if (wordCount < 3 || wordCount > 30)
            {
                await FollowupAsync("Count must be between 3 and 30 for a sensible sentence.");
                return;
            }

            var guildId = Context.Guild.Id;
            var model = GetOrBuildLanguageModel(guildId);

            if (model.Unigrams.Count == 0)
            {
                await FollowupAsync("Not enough data to build an average sentence.");
                return;
            }

            var rng = new Random();
            var sentence = GenerateAverageSentence(model, wordCount, rng);

            string[] words = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            int midpoint = (words.Length + 1) / 2;

            string line1 = string.Join(" ", words.Take(midpoint)) + ", ";
            string line2 = string.Join(" ", words.Skip(midpoint)) + ".";

            var firstLocation = new PointF(1050f, 800f);
            var secondLocation = new PointF(1050f, 1000f);

            var stream = File.OpenRead("Resources/hat.png");
            var bmp = new Bitmap(stream);

            using (var graphics = Graphics.FromImage(bmp))
            {
                var format = new StringFormat()
                {
                    Alignment = StringAlignment.Center
                };

                const float maxWidth = 1500f;

                using var font1 = CreateFittingFont(
                    graphics,
                    line1,
                    "Crimson Text",
                    FontStyle.Bold,
                    200,
                    60,
                    maxWidth);

                using var font2 = CreateFittingFont(
                    graphics,
                    line2,
                    "Crimson Text",
                    FontStyle.Bold,
                    200,
                    60,
                    maxWidth);

                graphics.DrawString(line1, font1, Brushes.Black, firstLocation, format);
                graphics.DrawString(line2, font2, Brushes.Black, secondLocation, format);

                var outputStream = new MemoryStream();
                bmp.Save(outputStream, System.Drawing.Imaging.ImageFormat.Png);

                await RespondWithFileAsync(outputStream, "hat.png");
            }
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

        [SlashCommand("alternativefact", "Retrieves an alternative fact")]
        public async Task GetAlternativeFact(int factId = -1)
        {
            var rng = new Random();
            AlternativeFact fact;
            await DeferAsync();

            if (factId == -1)
            {
                var factList = _alternativeFactRepository.GetAllFacts();
                fact = factList[rng.Next(factList.Count)];
            }
            else
            { 
                fact = _alternativeFactRepository.GetFact(factId);
            }

            if (fact == null)
            {
                await FollowupAsync($"No such fact exist.");
                return;
            }

            var author = _authorRepository.GetAuthor(fact.AuthorID);
            await FollowupAsync($"Fact #{fact.AlternativeFactID}: {fact.Description} - {author.Username}");
        }

        [SlashCommand("deletefact", "Deletes an alternative fact")]
        public async Task DeleteAlternativeFact(int factId)
        {
            await DeferAsync();
            var fact = _alternativeFactRepository.GetFact(factId);

            if (fact == null)
            {
                await FollowupAsync($"No such fact exist.");
            }
            else if (fact.AuthorID != Context.User.Id)
            {
                await FollowupAsync($"A fact can only be deleted by its author.");
            }
            else
            {
                _alternativeFactRepository.DeleteFact(factId);
                await FollowupAsync($"Fact #{fact.AlternativeFactID} successfully deleted");
            }
        }

        [SlashCommand("writefact", "Writes an alternative fact")]
        public async Task WriteFact(string fact)
        {
            await DeferAsync();
            if (fact.Length > 200)
            {
                await FollowupAsync("Please write facts that have maximum 200 characters");
                return;
            }
            _alternativeFactRepository.WriteFact(fact, Context.User.Id);
            await FollowupAsync($"Fact written successfully!");
        }

        [SlashCommand("reload", "Loads message history", runMode: RunMode.Async)]
        public async Task ReloadAsync(bool reloadSingleChannel = false, bool deleteHistory = false)
        {
            if (!Context.User.Username.Equals("thebali"))
            {
                await RespondAsync($"{MentionUtils.MentionUser(Context.User.Id)} you can't use that!");
                return;
            }

            // Respond immediately to avoid interaction timeout, then do the heavy work in background.
            await RespondAsync("Starting full message history reload (excluding threads). Progress will be posted here.");

            var guild = Context.Guild;
            var currentChannel = Context.Channel;

            _ = Task.Run(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IMessageRepository>();

                var channels = guild.TextChannels;
                int totalProcessed = 0;

                try
                {
                    if (deleteHistory)
                    {
                        if (reloadSingleChannel)
                            repo.DropMessages(currentChannel.Id);
                        else
                            repo.DropMessages(guild.Id);
                    }

                    foreach (var channel in channels)
                    {
                        if (reloadSingleChannel && channel.Id != currentChannel.Id) continue;

                        int channelProcessed = 0;

                        try
                        {
                            var mostRecentMessage = repo.GetMostRecentMessage(channel.Id);
                            mostRecentMessage ??= new Message { MessageID = 0 };
                            await foreach (var page in channel.GetMessagesAsync(mostRecentMessage.MessageID, Direction.After, int.MaxValue))
                            {
                                var batch = page
                                    .Where(x => !x.Author.IsBot && !x.ToString().StartsWith('$') && !x.ToString().StartsWith("p!c"))
                                    .ToList();

                                if (batch.Count > 0)
                                {
                                    repo.InsertBulkMessage(batch, guild);
                                    channelProcessed += batch.Count;
                                    totalProcessed += batch.Count;
                                }
                            }

                            await currentChannel.SendMessageAsync($"Loaded {channelProcessed} messages from #{channel.Name}.");
                        }
                        catch (Discord.Net.HttpException)
                        {
                            await currentChannel.SendMessageAsync($"I can't read #{channel.Name}.");
                        }
                    }

                    await currentChannel.SendMessageAsync($"Done loading {totalProcessed} messages!");
                }
                catch (Exception ex)
                {
                    await currentChannel.SendMessageAsync($"Reload failed: {ex.Message}");
                }
            });
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
                await RespondAsync("none of the above");
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

        [SlashCommand("togglequote", "Toggles wether the user is quotable or not")]
        public async Task ToggleQuote()
        {
            await DeferAsync();
            ulong authorID = Context.User.Id;
            var author = _authorRepository.GetAuthor(authorID);
            _authorRepository.ToggleQuotable(author);
            string status = author.IsQuotable ? "now" : "no longer";
            await FollowupAsync($"{Context.User.Username} is {status} quotable.");
        }

        [SlashCommand("quote", "Quotes someone at random, without context", runMode: RunMode.Async)]
        public async Task Quote(SocketGuildUser user = null)
        {
            
            await DeferAsync();
            var messageList = user == null ? 
                  _messageRepository.GetAllMessages(Context.Guild.Id) 
                : _messageRepository.GetAllMessages(Context.Guild.Id,user.Id);
            var rng = new Random();
            var index = rng.Next(messageList.Count);
            if (index <= 0)
            {
                await FollowupAsync($"This person either has no messages or doesn't wish to be quoted.");
                return;
            }

            var message = messageList[index];
            while (message.Content.Contains('@') || message.Content.Equals(""))
            {
                Console.WriteLine($"Tried to send {message.Content}");
                index = rng.Next(messageList.Count);
                message = messageList[index];
            }
            var author = _authorRepository.GetAuthor(message.AuthorID);
            SocketGuildUser authorObject = (SocketGuildUser)await Context.Channel.GetUserAsync(message.AuthorID);
            var displayName = authorObject != null ? authorObject.DisplayName : author.Username;
            DateTime date = DateTime.Parse(message.DateSent);
            long unixTimestamp = ((DateTimeOffset)date).ToUnixTimeSeconds();

            await FollowupAsync($"{message.Content} -{displayName}, <t:{unixTimestamp}:D>");
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
            List<string> results = [];
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
            Dictionary<string, int> dict = [];
            foreach (var message in messages)
            {
                var msg = message.Content;
                var res = Ngrams(size, msg);
                foreach (var gram in res)
                {
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
                    var cleanGram = Regex.Replace(gram, @"\p{Cs}", "");
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
                    if (dict.TryGetValue(cleanGram, out int value))
                    {
                        dict[cleanGram] = ++value;
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

        private List<string> Ngrams(int n, string str)
        {
            List<string> ngrams = [];
            string[] words = str.Split(" ");
            for (int i = 0; i < words.Length - n + 1; i++)
                ngrams.Add(Concat(words, i, i + n));
            return ngrams;
        }

        private static string Concat(string[] words, int start, int end)
        {
            StringBuilder sb = new();
            for (int i = start; i < end; i++)
                sb.Append((i > start ? " " : "") + words[i]);
            return sb.ToString();
        }

        private Dictionary<string, int> LoadMessages(int wordLength = 0)
        {
            var messages = _messageRepository.GetAllMessages(Context.Guild.Id);
            Dictionary<string, int> dict = [];
            foreach (var m in messages)
            {
                IEnumerable<string> words = [.. m.Content.Split(' ')];
                if (wordLength != 0)
                {
                    words = words.Where(x => x.Length == wordLength);
                }
                foreach (var w in words)
                {
                    if (dict.TryGetValue(w, out int value))
                    {
                        dict[w] = ++value;
                    }
                    else
                    {
                        dict[w] = 1;
                    }
                }
            }
            return dict;
        }

        // -------- Language Model Helpers --------

        private LanguageModelCache GetOrBuildLanguageModel(ulong guildId)
        {
            if (_languageModelCache.TryGetValue(guildId, out var cached))
            {
                // Rebuild occasionally
                if ((DateTime.UtcNow - cached.BuiltAt).TotalDays < 30)
                    return cached;
            }

            var messages = _messageRepository.GetAllMessages(guildId);
            Dictionary<string, int> unigrams = [];
            Dictionary<string, Dictionary<string, int>> bigrams = [];

            foreach (var msg in messages)
            {
                var tokens = Tokenize(msg.Content).ToList();
                if (tokens.Count == 0) continue;

                for (int i = 0; i < tokens.Count; i++)
                {
                    var w = tokens[i];
                    if (unigrams.TryGetValue(w, out int v))
                        unigrams[w] = v + 1;
                    else
                        unigrams[w] = 1;

                    if (i < tokens.Count - 1)
                    {
                        var next = tokens[i + 1];
                        if (!bigrams.TryGetValue(w, out var inner))
                        {
                            inner = [];
                            bigrams[w] = inner;
                        }
                        inner.TryGetValue(next, out int count);
                        inner[next] = count + 1;
                    }
                }
            }

            // Remove extremely rare words to reduce noise.
            unigrams = unigrams.Where(kv => kv.Value >= 2 && kv.Key.Length > 1).ToDictionary(x => x.Key, x => x.Value);

            var model = new LanguageModelCache
            {
                BuiltAt = DateTime.UtcNow,
                Unigrams = unigrams,
                Bigrams = bigrams,
                TotalUnigrams = unigrams.Values.Sum()
            };

            _languageModelCache[guildId] = model;
            return model;
        }

        private static IEnumerable<string> Tokenize(string content)
        {
            return content
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim())
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Where(w => !w.Contains("http"))
                .Select(w => Regex.Replace(w, @"^[\p{P}]+|[\p{P}]+$", "")) // strip leading/trailing punctuation
                .Where(w => w.Length > 0
                            && !w.Any(c => char.IsControl(c))
                            && !w.Contains('<')
                            && !w.StartsWith('@'))
                .Select(w => w.ToLowerInvariant());
        }

        private static string GenerateAverageSentence(LanguageModelCache model, int targetLength, Random rng)
        {
            if (model.Unigrams.Count == 0) return string.Empty;

            // Choose a starting word among top frequency lexical items (avoid filler like "the" sometimes).
            var startPool = model.Unigrams
                .OrderByDescending(kv => kv.Value)
                .Where(kv => kv.Key.Length > 2 && !IsStopWord(kv.Key))
                .Take(50)
                .Select(kv => kv.Key)
                .ToList();

            string current = startPool.Count > 0 ? startPool[rng.Next(startPool.Count)]
                                                 : model.Unigrams.OrderByDescending(kv => kv.Value).First().Key;

            List<string> words = [Capitalize(current)];
            HashSet<string> used = [];

            for (int i = 1; i < targetLength; i++)
            {
                string next = null;

                if (model.Bigrams.TryGetValue(current, out var nextDict))
                {
                    next = WeightedPick(nextDict, rng, w => !used.Contains(w));
                }

                if (next == null)
                {
                    // Fallback to overall distribution.
                    next = WeightedPick(model.Unigrams, rng, w => !used.Contains(w));
                }

                if (next == null) break;

                words.Add(next);
                used.Add(next);
                current = next;
            }

            // Basic smoothing: if last word doesn't end sentence punctuation, add a period.
            var last = words[^1];
            if (!Regex.IsMatch(last, @"[.!?]$"))
                words[^1] = last + ".";

            // Light post-format: capitalize first word, fix spacing.
            return string.Join(' ', words);
        }

        private static string WeightedPick(Dictionary<string, int> dict, Random rng, Func<string, bool> filter)
        {
            var filtered = dict.Where(kv => filter == null || filter(kv.Key)).ToList();
            if (filtered.Count == 0) return null;
            int total = filtered.Sum(kv => kv.Value);
            int roll = rng.Next(0, total);
            int cumulative = 0;
            foreach (var kv in filtered)
            {
                cumulative += kv.Value;
                if (roll < cumulative)
                    return kv.Key;
            }
            return filtered[^1].Key;
        }

        private static string WeightedPick(Dictionary<string, int> dict, Random rng)
        {
            return WeightedPick(dict, rng, _ => true);
        }

        private static bool IsStopWord(string w)
        {
            // Minimal list to reduce sentences starting with very common fillers.
            return w is "the" or "and" or "but" or "or" or "a" or "to" or "of";
        }

        private static string Capitalize(string w)
        {
            if (string.IsNullOrEmpty(w)) return w;
            if (w.Length == 1) return w.ToUpperInvariant();
            return char.ToUpperInvariant(w[0]) + w[1..];
        }

        private static Font CreateFittingFont(
        Graphics graphics,
        string text,
        string fontFamily,
        FontStyle fontStyle,
        float startingSize,
        float minimumSize,
        float maximumWidth)
        {
            float fontSize = startingSize;

            while (fontSize >= minimumSize)
            {
                var font = new Font(fontFamily, fontSize, fontStyle);

                SizeF size = graphics.MeasureString(text, font);

                if (size.Width <= maximumWidth)
                {
                    return font;
                }

                font.Dispose();
                fontSize -= 2f;
            }

            return new Font(fontFamily, minimumSize, fontStyle);
        }
    }

}
