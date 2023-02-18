using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Discord.Interactions;
using RunMode = Discord.Interactions.RunMode;
using Discord;

namespace BaliBotDotNet.Modules
{
    public class WebModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Dependency Injection will fill these values in for us
        public WebService WebService { get; set; }
        public IMessageRepository _messageRepository { get; set; }

        public WebModule(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        [SlashCommand("convertcurrency", "Converts from currency A to currency B.")]
        public async Task ConvertAsync(float amount, string source, string destination)
        {
            var rate = await WebService.GetConversionRateAsync(source, destination);
            if (rate == null)
            {
                await RespondAsync("Invalid currency or api is down");
            }
            else
            {
                await RespondAsync($"{amount} {source} is {string.Format("{0:0.00}", (amount * rate))} {destination}");
            }
        }

        [SlashCommand("8ball", "Predicts the future")]
        public async Task EightBall()
        {
            var random = new Random();
            List<string> answers = new()
            {
                "It is certain",
                "It is decidedly so",
                "Without a doubt",
                "Yes definitely",
                "You may rely on it",

                "As I see it, yes",
                "Most likely",
                "Outlook good",
                "Yes",
                "Signs point to yes",

                "Reply hazy, try again",
                "Ask again later",
                "Better not tell you now",
                "Cannot predict now",
                "Concentrate and ask again",

                "Don't count on it",
                "My reply is no",
                "My sources say no",
                "Outlook not so good",
                "Very doubtful"
            };
            await RespondAsync(answers[random.Next(answers.Count)]);
        }

        [SlashCommand("cat", "Gets a cat picture.")]
        public async Task CatAsync(string word = "")
        {
            await DeferAsync();
            var tuple = await WebService.GetCatPictureAsync(word);
            Stream stream = tuple.Item1;
            HttpStatusCode statusCode = tuple.Item2;
            if (stream == null && statusCode == HttpStatusCode.RequestTimeout)
            {
                await FollowupAsync("Cat API timed out :(");
                return;
            }

            if (!statusCode.HasFlag(HttpStatusCode.OK))
            {
                await FollowupAsync($"Api returned {((int)statusCode)} {statusCode}");
                return;
            }

            // Streams must be seeked to beginning before being uploaded!
            stream.Seek(0, SeekOrigin.Begin);
            if (word.Equals("gif"))
            {
                await FollowupWithFileAsync(stream, "cat.gif");
            }
            else
            {
                await FollowupWithFileAsync(stream, "cat.png");
            }
        }

        [SlashCommand("dog", "Gets a dog picture.")]
        public async Task DogAsync()
        {
            await DeferAsync();
            var stream = await WebService.GetDogPictureAsync();
            if (stream == null)
            {
                await FollowupAsync("Dog API timed out :(");
                return;
            }
            // Streams must be seeked to beginning before being uploaded!
            stream.Seek(0, SeekOrigin.Begin);
            await FollowupWithFileAsync(stream, "dog.png");
        }


        [SlashCommand("duck", "Gets a duck picture.")]
        public async Task DuckAsync()
        {
            await DeferAsync();
            var stream = await WebService.GetDuckPictureAsync();
            if (stream == null)
            {
                await RespondAsync("Duck API timed out :(");
                return;
            }
            // Streams must be seeked to beginning before being uploaded!
            stream.Seek(0, SeekOrigin.Begin);
            await FollowupWithFileAsync(stream, "duck.png");
        }


        [SlashCommand("fox", "Gets a fox picture.")]
        public async Task FoxAsync()
        {
            await DeferAsync();
            var stream = await WebService.GetFoxPictureAsync();
            if (stream == null)
            {
                await RespondAsync("Fox API timed out :(");
                return;
            }
            // Streams must be seeked to beginning before being uploaded!
            stream.Seek(0, SeekOrigin.Begin);
            await FollowupWithFileAsync(stream, "fox.png");
        }

        [SlashCommand("dadjoke", "Gets a dad joke")]
        public async Task DadJokeAsync()
        {
            string joke = await WebService.GetDadJokeAsync();
            await RespondAsync(joke);
        }

        [SlashCommand("xkcd", "Gets an XKCD comic.")]
        public async Task XKCDAsync(string xkcdID = null)
        {
            XKCDContainer container;

            if (xkcdID == null)
            {
                container = await WebService.GetXKCDAsync(null, true);
            }
            else if (xkcdID == "last" || xkcdID == "latest")
            {
                container = await WebService.GetXKCDAsync(null);
            }
            else if (!int.TryParse(xkcdID, out int id) || id < 1)
            {
                await RespondAsync($"{xkcdID} is not a valid XKCD comic ID.");
                return;
            }
            else
            {
                container = await WebService.GetXKCDAsync(id);
            }

            if (container == null)
            {
                await RespondAsync($"There is no comic matching id {xkcdID}");
            }
            else
            {
                // Streams must be seeked to beginning before being uploaded!
                container.Image.Seek(0, SeekOrigin.Begin);
                await RespondAsync("#" + container.ID + ": " + container.Title);
                await Context.Channel.SendFileAsync(container.Image, "xkcd.png");
                await Context.Channel.SendMessageAsync("Alt text: " + container.AltText);
            }
        }
    }
}
