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

        [SlashCommand("convertcurrency", "Converts from currency A to currency B. Example usage : `$convertcurrency 100 eur cad.`")]
        public async Task ConvertAsync(float amount, string source, string destination)
        {
            var rate = await WebService.GetConversionRateAsync(source, destination);
            if (rate == null)
            {
                await Context.Channel.SendMessageAsync("Invalid currency or api is down");
            }
            else
            {
                await Context.Channel.SendMessageAsync($"{amount} {source} is {string.Format("{0:0.00}", (amount * rate))} {destination}");
            }
        }

        [SlashCommand("8ball","Predicts the future")]
        public async Task EightBall(params string[] question)
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
            await ReplyAsync(answers[random.Next(answers.Count)]);
        }

        [SlashCommand("dailypuzzle", "Gets the daily puzzle on Lichess", runMode: RunMode.Async)]
        public async Task DailyPuzzle()
        {
            //var puzzle = await WebService.GetLichessPuzzle();
            //await Context.Channel.SendFileAsync(puzzle.Image, "lichess.gif");
            //await Context.Channel.SendMessageAsync($"{Context.User.Username}, you can reply moves to the puzzle by using a \"e2e4\" format. Type `stop` to stop the bot from tracking your answers.");
            //var solution = puzzle.Solution;

            //var regex = new Regex("([a-h][1-8]){2}");
            //while (solution.Count > 0)
            //{
            //    var message = await NextMessageAsync(true,true,TimeSpan.FromMinutes(1));
            //    if (message == null || !regex.IsMatch(message.Content))
            //    {
            //        continue;
            //    }
               
            //    if (message.Content.Equals("stop"))
            //    {
            //        break;
            //    }
            //    else if (message.Content.Equals(solution[0]))
            //    {
            //        if (solution.Count == 1)
            //        {
            //            await Context.Channel.SendMessageAsync("You solved the puzzle!");
            //            solution.RemoveAt(0);
            //        }
            //        else
            //        {
            //            await Context.Channel.SendMessageAsync($"That is the correct move! Opponent plays {solution[1]}");
            //            solution.RemoveAt(0);
            //            solution.RemoveAt(0);
            //        }
            //    }
            //    else
            //    {
            //        await Context.Channel.SendMessageAsync("That is not the correct move!");
            //    }
            //}

        }

        [SlashCommand("cat", "Gets a cat picture. You can use `$cat gif` to get a gif or `$cat cute` to get a cute cat. Any other words are ignored.")]
        public async Task CatAsync(string word = "")
        {
            var tuple = await WebService.GetCatPictureAsync(word);
            Stream stream = tuple.Item1;
            HttpStatusCode statusCode = tuple.Item2;
            if (stream == null && statusCode == HttpStatusCode.RequestTimeout)
            {
                await Context.Channel.SendMessageAsync("Cat API timed out :(");
                return;
            }

            if (!statusCode.HasFlag(HttpStatusCode.OK))
            {
                await Context.Channel.SendMessageAsync($"Api returned {((int)statusCode)} {statusCode}");
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

        [SlashCommand("dog", "Gets a dog picture.")]
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


        [SlashCommand("duck", "Gets a duck picture.")]
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


        [SlashCommand("fox", "Gets a fox picture.")]
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

        [SlashCommand("dadjoke", "Gets a dad joke")]
        public async Task DadJokeAsync()
        {
            string joke = await WebService.GetDadJokeAsync();
            await ReplyAsync(joke);
        }

        [SlashCommand("xkcd", "Gets a random XKCD comic, or a specific one if an ID is specificed. Can also fetch the last comic if  \"last\" or \"latest\" is specified as ID.")]
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
