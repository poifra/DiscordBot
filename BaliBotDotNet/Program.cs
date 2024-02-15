using BaliBotDotNet.Data;
using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Model;
using BaliBotDotNet.Services;
using BaliBotDotNet.Utilities.UOM;
using BalibotTest.MeasurementResolving;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;

namespace BaliBotDotNet
{
    public class Program
    {
        readonly UOMConverter Converter = new();
        IMessageRepository _messageRepository;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _services;
        private char? forbiddenLetter = null;
        private readonly DiscordSocketConfig _socketConfig = new()
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.MessageContent,
            AlwaysDownloadUsers = true,
            UseInteractionSnowflakeDate = false
        };

        public Program()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("config.json")
                .Build();
            _services = new ServiceCollection()
                .AddSingleton(_configuration)
                .AddSingleton(_socketConfig)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<CommandService>()
                .AddSingleton<InteractionHandler>()
                .AddSingleton<HttpClient>()
                .AddSingleton<WebService>()
                .AddSingleton<IMessageRepository, MessageRepository>()
                .AddSingleton<IReminderRepository, ReminderRepository>()
                .AddSingleton<IAuthorRepository, AuthorRepository>()
                .BuildServiceProvider();
            MeasurementConversionHandler.GenerateAvailableMeasurementsList();
            _messageRepository = new MessageRepository();
        }
        static void Main(string[] args)
            => new Program()
            .RunAsync()
            .GetAwaiter()
            .GetResult();

        private async Task RunAsync()
        {
            var client = _services.GetRequiredService<DiscordSocketClient>();

            client.Log += LogAsync;
            client.MessageReceived += MessageHandler;
            _services.GetRequiredService<CommandService>().Log += LogAsync;

            await client.LoginAsync(TokenType.Bot, _configuration["token"]);
            await client.StartAsync();

            //Initialize the logic required to register commands.
            await _services.GetRequiredService<InteractionHandler>()
                .InitializeAsync();
            await Task.Delay(Timeout.Infinite);
        }

        private async Task MessageHandler(SocketMessage message)
        {
            if (message.Author.Username == "subpixelmaster4000")
            {
                return;
            }
            if (message.Source != MessageSource.User) //bot doesnt reply to other bots (including itself)
            {
                return;
            }
            if (!message.Content.StartsWith("$"))
            {
                SocketGuild guild = (message.Channel as SocketGuildChannel)?.Guild;
                if (guild != null) // if its not a dm
                {
                    _messageRepository.InsertMessage(message, guild);
                }
            }
            var date = DateTime.Now;
            if (date.Month == 11 && date.Day == 29)
            {
                if (forbiddenLetter == null)
                {
                    char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToLower().ToCharArray();
                    var rng = new Random();
                    var forbidden = alpha[rng.Next(alpha.Length)];
                    forbiddenLetter = forbidden;
                }
                if (message.Content.Contains(forbiddenLetter.Value))
                {
                    await message.Channel.DeleteMessageAsync(message);
                }
            }
          
            if (message.Content.ToLower().Contains("thanks balibot"))
            {
                await message.Channel.SendMessageAsync("You're welcome!");
            }

            //filter spoilers
            var regexResult = message.Content.Contains("||") ? null : MeasurementMessageHandler.TryConvertMessage(message.Content);

            if (regexResult != null)
            {
                await message.Channel.SendMessageAsync(regexResult);
            }
        }
        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        public static bool IsDebug()
        {
#if DEBUG
            return true;
#else
                return false;
#endif
        }
    }
}