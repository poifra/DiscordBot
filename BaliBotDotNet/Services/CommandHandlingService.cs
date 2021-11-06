using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
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
        private Timer _timer;
        private bool _canUseCommand;

        public CommandHandlingService(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
            _timer = new Timer(60 * 1000);
            _canUseCommand = true;
            _timer.Elapsed += ResetLimit;

            _commands.CommandExecuted += CommandExecutedAsync;
            _discord.MessageReceived += MessageReceivedAsync;
        }

        private void ResetLimit(object sender, ElapsedEventArgs e)
        {
            _canUseCommand = true;
            _timer.Stop();
        }

        public async Task InitializeAsync()
        {
            // Register modules that are public and inherit ModuleBase<T>.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

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

            bool isDebug = false;
            if (_canUseCommand || isDebug)
            {
                await _commands.ExecuteAsync(context, argPos, _services);
                _canUseCommand = false;
                _timer.Start();
            }
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // command is unspecified when there was a search failure (command not found); we don't care about these errors
            if (!command.IsSpecified)
                return;

            // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess)
                return;

            // the command failed, let's notify the user that something happened.
            await context.Channel.SendMessageAsync($"error: {result}");
        }
    }
}
