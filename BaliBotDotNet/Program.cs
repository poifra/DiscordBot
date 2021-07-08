using BaliBotDotNet.Data;
using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Model;
using BaliBotDotNet.Services;
using BaliBotDotNet.Utilities.UOM;
using BalibotTest.MeasurementResolving;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BaliBotDotNet
{
    class Program
    {
        UOMConverter Converter = new UOMConverter();
        IMessageRepository _messageRepository;
        static void Main(string[] args)
        {
            MeasurementConversionHandler.GenerateAvailableMeasurementsList();
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            InitializeDB();
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

        private void InitializeDB()
        {
            _messageRepository = new MessageRepository();
        }

        private async Task MessageHandler(SocketMessage message)
        {
            if (message.Source != MessageSource.User) //bot doesnt reply to other bots (including itself)
            {
                return;
            }

            if (!message.Content.StartsWith("$"))
            {
                SocketGuild guild = ((SocketGuildChannel)message.Channel).Guild;
                _messageRepository.InsertMessage(message, guild);
            }

            var regexResult = MeasurementMessageHandler.TryConvertMessage(message.Content);

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

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .AddSingleton<WebService>()
                .AddSingleton<IMessageRepository, MessageRepository>()
                .AddSingleton<IReminderRepository, ReminderRepository>()
                .BuildServiceProvider();
        }
    }
}