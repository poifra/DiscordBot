using BaliBotDotNet.Utilities.Helpers;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;

namespace BaliBotDotNet.Services
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _handler;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;

        public const char Prefix = '$';
        private readonly Dictionary<ulong, TimerContext> _timerContextByServerID;
        private const int delay_in_seconds = 30;
        public InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services, IConfiguration config)
        {
            _client = client;
            _handler = handler;
            _services = services;
            _configuration = config;
            _timerContextByServerID = new Dictionary<ulong, TimerContext>();
        }

        public async Task InitializeAsync()
        {
            _client.Ready += ReadyAsync;
            _handler.Log += LogAsync;

            await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // Process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;
            _client.MessageReceived += MessageReceivedAsync;
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            if (interaction.User.Username == "subpixelmaster4000")
            {
                return;
            }
   
            try
            {
                var context = new SocketInteractionContext(_client, interaction);
                var result = await _handler.ExecuteCommandAsync(context, _services);
            }
            catch
            {
                if (interaction.Type is InteractionType.ApplicationCommand)
                    await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }

        private async Task LogAsync(LogMessage log) => Console.WriteLine(log);

        private async Task ReadyAsync()
        {
            // Context & Slash commands can be automatically registered, but this process needs to happen after the client enters the READY state.
            // Since Global Commands take around 1 hour to register, we should use a test guild to instantly update and test our commands.
            if (Program.IsDebug())
            {
                await _handler.RegisterCommandsToGuildAsync(ulong.Parse(_configuration["testGuild"]),true);
                await _handler.RegisterCommandsToGuildAsync(ulong.Parse(_configuration["ragnacord"]),true);
            }
            else
                await _handler.RegisterCommandsGloballyAsync(true);
        }

        public async Task MessageReceivedAsync(SocketMessage message)
        {

            //This value holds the offset where the prefix ends
           var text = message.Content.ToLower();
            if (text.Contains("i'm") || text.Contains("i am"))
            {
                var rng = new Random();
                if (rng.Next(1000) == 420)
                {
                    var dadJokeIndex = text.IndexOf("i'm");
                    if (dadJokeIndex == -1)
                    {
                        dadJokeIndex = text.IndexOf("i am");
                    }
                    var name = text[(dadJokeIndex + 4)..];
                    await message.Channel.SendMessageAsync($"Hi {name}! I'm BaliBot!");
                }
            }

            //Discord.Interactions.IResult result;
            //var context = Context.;
            //if (context.Channel.GetChannelType() == ChannelType.DM) //no need for a cooldown in DMs
            //{
            //    result = await _handler.ExecuteCommandAsync(context, _services);
            //    return;
            //}

            //ulong serverID = context.Guild.Id;

            //if (!_timerContextByServerID.ContainsKey(serverID))
            //{
            //    var timerInstance = new TimerContext { CanUseCommand = true, ServerID = serverID, Interval = delay_in_seconds * 1000 };
            //    timerInstance.Elapsed += ResetLimit;
            //    _timerContextByServerID[serverID] = timerInstance;
            //}

            //bool isDebug = false;
            //if (!_timerContextByServerID[serverID].CanUseCommand && !isDebug) // always execute commands if isDebug is set to true
            //{
            //    var dm = await context.User.CreateDMChannelAsync();
            //    double timeRemaining = (delay_in_seconds - (DateTime.Now - _timerContextByServerID[serverID].StartTime).TotalSeconds);
            //    await dm.SendMessageAsync($"The bot is on cooldown! Please wait {string.Format("{0:0.00}", timeRemaining)} seconds before using a command.");
            //    return;
            //}

            //result = await _handler.ExecuteCommandAsync(context, _services);
            //if (result.IsSuccess)
            //{
            //    _timerContextByServerID[serverID].Start();
            //    _timerContextByServerID[serverID].CanUseCommand = false;
            //    _timerContextByServerID[serverID].StartTime = DateTime.Now;
            //}
        }

        private void ResetLimit(object sender, ElapsedEventArgs e)
        {
            TimerContext context = (TimerContext)sender;
            context.CanUseCommand = true;
            context.Stop();
        }
    }
}

