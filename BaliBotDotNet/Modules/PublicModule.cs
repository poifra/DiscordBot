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
        public async Task CatAsync()
        {
            var stream = await WebService.GetCatPictureAsync();
            // Streams must be seeked to beginning before being uploaded!
            stream.Seek(0, SeekOrigin.Begin);
            await Context.Channel.SendFileAsync(stream, "cat.png");
        }

        [Command("xkcd")]
        public async Task XKCDAsync(string xkcdID = null)
        {
            XKCDContainer container;

            //TODO: random comic
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
                await Context.Channel.SendMessageAsync("Title: " + container.Title);
                await Context.Channel.SendFileAsync(container.Image, "xkcd.png");
                await Context.Channel.SendMessageAsync("Alt text: " + container.AltText);
            }
        }

        // Get info on a user, or the user who invoked the command if one is not specified
        [Command("userinfo")]
        public async Task UserInfoAsync(IUser user = null)
        {
            user ??= Context.User;
            await ReplyAsync(user.ToString());
        }

        [Command("logout")]
        public async Task LogoutAsync2()
        {
            await ReplyAsync("Later nerds, I'm going to bed!");
            await Context.Client.LogoutAsync();
            Environment.Exit(0);
        }
    }
}
