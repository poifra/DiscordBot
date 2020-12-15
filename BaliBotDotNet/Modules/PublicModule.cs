using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Services;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BaliBotDotNet.Modules
{
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        // Dependency Injection will fill this value in for us
        public WebService WebService { get; set; }
        public IMessageRepository _messageRepository { get; set; }

        public PublicModule(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        [Command("ping")]
        [Alias("pong", "hello")]
        public Task PingAsync()
            => ReplyAsync("pong!");

        [Command("cat")]
        [Summary("Gets a cat picture")]
        public async Task CatAsync(string word = "")
        {
            var stream = await WebService.GetCatPictureAsync(word);
            if (stream == null)
            {
                await Context.Channel.SendMessageAsync("Cat API timed out :(");
                return;
            }
            // Streams must be seeked to beginning before being uploaded!
            stream.Seek(0, SeekOrigin.Begin);
            if (word.Equals("gif"))
            {
                await Context.Channel.SendFileAsync(stream, "cat.gif");
            }
            else
            {
                await Context.Channel.SendFileAsync(stream, "cat.png");
            }
        }

        [Command("dog")]
        [Summary("joke")]
        public async Task DogAsync()
        {
            var stream = await WebService.GetDogPictureAsync();
            if (stream == null)
            {
                await Context.Channel.SendMessageAsync("Dog API timed out :(");
                return;
            }
            // Streams must be seeked to beginning before being uploaded!
            stream.Seek(0, SeekOrigin.Begin);
            await Context.Channel.SendFileAsync(stream, "dog.png");
        }

        [Command("xkcd")]
        [Summary("Gets a random XKCD comic, or a specific one if an ID is specificed. Can also fetch the last comic if  \"last\" is specified as ID.")]
        public async Task XKCDAsync(string xkcdID = null)
        {
            XKCDContainer container;

            if (xkcdID == null)
            {
                container = await WebService.GetXKCDAsync(null, true);
            }
            else if (xkcdID == "last")
            {
                container = await WebService.GetXKCDAsync(null);
            }
            else if (!int.TryParse(xkcdID, out int id) || id < 1)
            {
                await Context.Channel.SendMessageAsync($"{xkcdID} is not a valid XKCD comic ID.");
                return;
            }
            else
            {
                container = await WebService.GetXKCDAsync(id);
            }

            if (container == null)
            {
                await Context.Channel.SendMessageAsync($"There is no comic matching id {xkcdID}");
            }
            else
            {
                // Streams must be seeked to beginning before being uploaded!
                container.Image.Seek(0, SeekOrigin.Begin);
                await Context.Channel.SendMessageAsync("#" + container.ID + ": " + container.Title);
                await Context.Channel.SendFileAsync(container.Image, "xkcd.png");
                await Context.Channel.SendMessageAsync("Alt text: " + container.AltText);
            }
        }

        [Command("userinfo")]
        [Summary("Get info on a user, or the user who invoked the command if one is not specified")]
        public async Task UserInfoAsync(IUser user = null)
        {
            user ??= Context.User;
            await ReplyAsync(user.ToString());
        }

        [Command("reload", RunMode = RunMode.Async)]
        public async Task ReloadAsync()
        {
            if (!Context.Message.Author.Username.Equals("Bali"))
            {
                await ReplyAsync(MentionUtils.MentionUser(Context.Message.Author.Id) + " you can't use that!");
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
                catch(Discord.Net.HttpException _)
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
                await ReplyAsync(MentionUtils.MentionUser(Context.Message.Author.Id) + " you can't use that!");
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
                ulong mention = 0;
                if (MentionUtils.TryParseUser(kv.Key, out mention))
                {
                    await ReplyAsync("This would ping someone :(");
                }
                else
                {
                    await ReplyAsync($"The most common word with {wordLength} letters is \"{kv.Key}\" with {kv.Value} occurences."); 
                }
                
            }

        }
        [Command("logout")]
        [Summary("Logs out the bot.")]
        public async Task LogoutAsync2()
        {
            await ReplyAsync("Later nerds, I'm going to bed!");
            await Context.Client.LogoutAsync();
            Environment.Exit(0);
        }
    }
}
