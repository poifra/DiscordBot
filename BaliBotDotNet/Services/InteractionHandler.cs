using BaliBotDotNet.Utilities.Helpers;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BaliBotDotNet.Services
{
    public class InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services, IConfiguration config)
    {
        private readonly DiscordSocketClient _client = client;
        private readonly InteractionService _handler = handler;
        private readonly IServiceProvider _services = services;
        private readonly IConfiguration _configuration = config;

        #region Compliments 

        private static readonly string[] Compliments =
        {
            "{0}, your APM is so high, even AI struggles to keep up!",
            "{0}, you're the grandmaster of good vibes and checkmates!",
            "If chess had a 'style' rating, {0}, you'd be off the charts!",
            "{0}, your micro-management is so good, even Protoss players take notes!",
            "{0}, you're a legendary Pokémon—rare, powerful, and absolutely adorable!",
            "{0}, your gender identity is as valid as a 200 IQ gambit!",
            "{0}, you have the reaction speed of a pro gamer and the kindness of a Pokémon Center nurse!",
            "{0}, you're so strategic, even Sun Tzu would ask for your advice!",
            "{0}, your presence in a lobby makes every match 200% more fun!",
            "If cuteness were an RTS unit, {0}, you'd be max-tier meta!",
            "{0}, you could speedrun my heart in record time!",
            "Even a Magikarp would evolve instantly in your inspiring presence, {0}!",
            "{0}, your smile has a crit chance of 100%—it's always super effective!",
            "{0}, you're like a shiny Pokémon—rare, special, and incredibly valuable!",
            "{0}, your ability to adapt in games is more impressive than a Ditto in a tournament!",
            "{0}, you're so good at chess, even your blunders turn into brilliant sacrifices!",
            "{0}, your stratagems are so next-level, even AI bots call you 'senpai'!",
            "{0}, you're cuter than a basket of sleeping puppies and twice as heartwarming!",
            "If life were an RTS, {0}, you'd already be four expansions ahead!",
            "{0}, your positivity is as contagious as a Zerg rush!",
            "{0}, your presence makes every party more OP than a well-fed ADC!",
            "{0}, you're the critical hit in my otherwise average damage output!",
            "{0}, you’ve got the charisma stat of a fully maxed-out RPG protagonist!",
            "{0}, your kindness is more overpowered than an unpatched exploit!",
            "{0}, you're the Eevee of people—versatile, adorable, and loved by all!",
            "Even Ditto couldn't copy how amazing you are, {0}!",
            "{0}, you're rarer than a wild Mew under a truck!",
            "{0}, you light up the room like a Voltorb—except in a good way!",
            "{0}, you make the world a better place just by being in it!",
            "{0}, your patience is legendary—like a wise sage guiding others!",
            "{0}, you're as resilient as a tree standing tall through every storm!",
            "{0}, your words have the power to heal like the best-written stories!",
            "{0}, you're the type of person who makes even the smallest moments special!",
            "{0}, your presence is as comforting as a familiar song from childhood!",
            "{0}, you're the perfect mix of wisdom, humor, and kindness!",
            "{0}, you have the strength to face any challenge and the heart to help others do the same!",
            "{0}, your kindness is like a ripple that turns into a wave of positivity!",
            "{0}, you make the world brighter, one thoughtful action at a time!",
            "{0}, your smile could power an entire city if we could harness its brilliance!",
            "{0}, your eyes are so captivating, I almost forgot what I was saying!",
            "{0}, your hair looks so effortlessly perfect, I suspect magic might be involved!",
            "{0}, your skin glows like you have a personal lighting crew following you around!",
            "{0}, your ears are so cute, I bet even earrings feel honored to sit there!",
            "{0}, your face is so symmetrical, even mirrors are impressed!",
        };

        #endregion

        public const char Prefix = '$';
        
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
                await _handler.RegisterCommandsToGuildAsync(ulong.Parse(_configuration["testguild"]),true);
                await _handler.RegisterCommandsToGuildAsync(ulong.Parse(_configuration["ragnacord"]),true);
                await _handler.RegisterCommandsToGuildAsync(ulong.Parse(_configuration["greencord"]),true);
            }
            else
                await _handler.RegisterCommandsGloballyAsync(true);
        }

        public async Task MessageReceivedAsync(SocketMessage message)
        {

            //This value holds the offset where the prefix ends
            var text = message.Content.ToLower();
            var rng = new Random();

            if (rng.Next(1000000) == 69)
            {
                await message.Channel.SendMessageAsync(GetLotteryMessage(message.Author.GlobalName));
                return;
            }

            if (text.Contains("i'm") || text.Contains("i am"))
            {
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
        }

        private string GetLotteryMessage(string userName)
        {
            var sb = new StringBuilder();
            var random = new Random();

            sb.AppendLine($"This message has a 1 / 1000000 chance to appear.Consider yourself lucky {userName}.");
            sb.AppendLine("You won the Bali-Bot Lottery and get one (1) compliment as a reward.");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(string.Format(Compliments[random.Next(Compliments.Length)], userName));

           return sb.ToString();
        }

        private void ResetLimit(object sender, ElapsedEventArgs e)
        {
            TimerContext context = (TimerContext)sender;
            context.CanUseCommand = true;
            context.Stop();
        }
    }
}

