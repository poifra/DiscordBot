using BaliBotDotNet.Services;
using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BaliBotDotNet.Modules
{
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        // Dependency Injection will fill this value in for us
        public WebService WebService { get; set; }

        [Command("ping")]
        [Alias("pong", "hello")]
        public Task PingAsync()
            => ReplyAsync("pong!");

        [Command("cat")]
        [Summary("Gets a cat picture")]
        public async Task CatAsync(string word = null)
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
            await ReplyAsync("Find me a dog API and I'll do it.");
            await ReplyAsync(":dog:");
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
