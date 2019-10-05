using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using BaliBotDotNet.Services;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text.Json;

namespace BaliBotDotNet
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            using var services = ConfigureServices();
            var client = services.GetRequiredService<DiscordSocketClient>();
            client.Log += LogAsync;
            client.MessageReceived += MessageHandler;

            services.GetRequiredService<CommandService>().Log += LogAsync;

            using var document = JsonDocument.Parse(File.ReadAllText("../../../config.json"));
            string token = document.RootElement.GetProperty("token").GetString();
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            //Initialize the logic required to register commands.
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync();
            await Task.Delay(-1);
        }

        private async Task MessageHandler(SocketMessage message)
        {
            if (message.Source != MessageSource.User) //bot doesnt reply to other bots (including itself)
            {
                return;
            }
            Regex regex = new Regex(@"-?.\d*\.?\d* .(ft|mi|c|f|kg|lb|km|m) *", RegexOptions.IgnoreCase);
            var matches = regex.Matches(message.Content);
            var unit = matches.FirstOrDefault()?.Groups.Values.Last().Value;
            var match = matches.FirstOrDefault()?.ToString();

            if (unit == null 
                || match == null
                || !double.TryParse(match.Substring(0, match.Length - unit.Length), NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
            {
                return;
            }

            unit = unit.ToLowerInvariant();
            _ = (unit switch
            {
                "ft" => await message.Channel.SendMessageAsync(match + " is " + (number * 0.3054).ToString("N", CultureInfo.InvariantCulture) + " meters."),
                "m" => await message.Channel.SendMessageAsync(match + " is " + (number / 0.3054).ToString("N", CultureInfo.InvariantCulture) + " feet."),
                "c" => await message.Channel.SendMessageAsync(match + " is " + (number * (9 / 5) + 32).ToString("N", CultureInfo.InvariantCulture) + " fahreinheit"),
                "f" => await message.Channel.SendMessageAsync(match + " is " + ((number - 32) / (9.0 / 5.0)).ToString("N", CultureInfo.InvariantCulture) + " celsius."),
                "kg" => await message.Channel.SendMessageAsync(match + " is " + (number * 2.2046).ToString("N", CultureInfo.InvariantCulture) + " pounds."),
                "lb" => await message.Channel.SendMessageAsync(match + " is " + (number / 2.2046).ToString("N", CultureInfo.InvariantCulture) + " kilograms."),
                "km" => await message.Channel.SendMessageAsync(match + " is " + (number / 1.6093).ToString("N", CultureInfo.InvariantCulture) + " miles."),
                "mi" => await message.Channel.SendMessageAsync(match + " is " + (number * 1.6093).ToString("N", CultureInfo.InvariantCulture) + " kilometers."),
                _ => null,
            });
        }
        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .AddSingleton<PictureService>()
                .BuildServiceProvider();
        }
    }
}