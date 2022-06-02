﻿using BaliBotDotNet.Data;
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

        private readonly DiscordSocketConfig _socketConfig = new()
        {
            GatewayIntents = GatewayIntents.GuildMessages
                | GatewayIntents.GuildMessageReactions
                | GatewayIntents.GuildEmojis
                | GatewayIntents.GuildMembers
                | GatewayIntents.GuildVoiceStates
                | GatewayIntents.DirectMessageReactions
                | GatewayIntents.DirectMessageTyping
                | GatewayIntents.DirectMessages
                | GatewayIntents.GuildEmojis
                | GatewayIntents.Guilds,
            AlwaysDownloadUsers = true
        };

        public Program()
        {
            _configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "$")
                .AddJsonFile("config.json", optional: true)
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
        }
        static void Main(string[] args) 
            => new Program()
            .RunAsync()
            .GetAwaiter()
            .GetResult();

        private async Task RunAsync()
        {
            InitializeDB();

            var client = _services.GetRequiredService<DiscordSocketClient>();

            client.Log += LogAsync;
            client.MessageReceived += MessageHandler;
            _services.GetRequiredService<CommandService>().Log += LogAsync;

            await client.LoginAsync(TokenType.Bot, _configuration["token"]);
            await client.StartAsync();

            //Initialize the logic required to register commands.
            await _services.GetRequiredService<InteractionHandler>()
                .InitializeAsync();
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
                SocketGuild guild = (message.Channel as SocketGuildChannel)?.Guild;
                if (guild != null) // if its not a dm
                {
                    _messageRepository.InsertMessage(message, guild);
                }
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