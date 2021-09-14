using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Services;
using Discord.Commands;
using System.IO;
using System.Threading.Tasks;

namespace BaliBotDotNet.Modules
{
    public class WebModule : ModuleBase<SocketCommandContext>
    {
        // Dependency Injection will fill these values in for us
        public WebService WebService { get; set; }
        public IMessageRepository _messageRepository { get; set; }

        public WebModule(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

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
        [Summary("Woof")]
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


        [Command("duck")]
        [Summary("Quack")]
        public async Task DuckAsync()
        {
            var stream = await WebService.GetDuckPictureAsync();
            if (stream == null)
            {
                await Context.Channel.SendMessageAsync("Duck API timed out :(");
                return;
            }
            // Streams must be seeked to beginning before being uploaded!
            stream.Seek(0, SeekOrigin.Begin);
            await Context.Channel.SendFileAsync(stream, "duck.png");
        }


        [Command("fox")]
        [Summary("Floof")]
        public async Task FoxAsync()
        {
            var stream = await WebService.GetFoxPictureAsync();
            if (stream == null)
            {
                await Context.Channel.SendMessageAsync("Fox API timed out :(");
                return;
            }
            // Streams must be seeked to beginning before being uploaded!
            stream.Seek(0, SeekOrigin.Begin);
            await Context.Channel.SendFileAsync(stream, "fox.png");
        }

        [Command("dadjoke")]
        public async Task DadJokeAsync()
        {
            string joke = await WebService.GetDadJokeAsync();
            await ReplyAsync(joke);
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
    }
}
