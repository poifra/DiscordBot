using BaliBotDotNet.Utilities.Helpers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;

namespace BaliBotDotNet.Services
{
    class CommandHandlingService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        public const char Prefix = '$';
        private readonly Dictionary<ulong, TimerContext> _timerContextByServerID;

        public CommandHandlingService(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
            _timerContextByServerID = new Dictionary<ulong, TimerContext>();

            _commands.CommandExecuted += CommandExecutedAsync;
            _discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            // Register modules that are public and inherit ModuleBase<T>.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (rawMessage is not SocketUserMessage message)
            {
                return;
            }
            if (message.Source != MessageSource.User)
            {
                return;
            }

            // This value holds the offset where the prefix ends
            var argPos = 0;

            if (!message.HasCharPrefix(Prefix, ref argPos))
            {
                return;
            }

            if (message.Author.ToString().Equals("subpixelmaster4000#4495"))
            {
                return;
            }

            var context = new SocketCommandContext(_discord, message);
            ulong serverID = context.Guild.Id;

            var rng = new Random();
            int n = rng.Next(0, 10_001);
            if (n == 10_000)
            {
                await context.Channel.SendMessageAsync("lol cringe");
            }

            if (!_timerContextByServerID.ContainsKey(serverID))
            {
                var timerInstance = new TimerContext { CanUseCommand = true, ServerID = serverID, Interval = 60 * 1000 };
                timerInstance.Elapsed += ResetLimit;
                _timerContextByServerID[serverID] = timerInstance;
            }

            bool isDebug = false;
            if (!_timerContextByServerID[serverID].CanUseCommand && !isDebug) // always execute commands if isDebug is set to true
            {
                var dm = await context.User.GetOrCreateDMChannelAsync();
                await dm.SendMessageAsync($"The bot is on cooldown! Please wait at least one minute");
                return;
            }

            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (result.IsSuccess)
            {
                _timerContextByServerID[serverID].Start();
                _timerContextByServerID[serverID].CanUseCommand = false;
            }
        }

        private void ResetLimit(object sender, ElapsedEventArgs e)
        {
            TimerContext context = (TimerContext)sender;
            context.CanUseCommand = true;
            context.Stop();
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // command is unspecified when there was a search failure (command not found); we don't care about these errors
            if (!command.IsSpecified)
            {
                return;
            }

            // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess)
            {
                return;
            }

            // the command failed, let's notify the user that something happened.
            await context.Channel.SendMessageAsync($"error: {result}");
        }
    }
}

