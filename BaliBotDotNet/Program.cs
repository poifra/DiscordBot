using BaliBotDotNet.Data;
using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Models;
using BaliBotDotNet.Services;
using BalibotTest.MeasurementResolving;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BaliBotDotNet
{
    public class Program
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _services;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DiscordSocketConfig _socketConfig = new()
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.MessageContent,
            AlwaysDownloadUsers = true,
            UseInteractionSnowflakeDate = false
        };

        // Track a forbidden letter per guild for April 1st, thread-safe
        private readonly ConcurrentDictionary<ulong, char> _forbiddenLettersByGuild = new();

        public Program()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("config.json")
                .Build();
            var interactionConfig = new InteractionServiceConfig()
            {
                LocalizationManager = new JsonLocalizationManager(basePath: "Resources", fileName: "commands")
            };

            _services = new ServiceCollection()
                .AddSingleton(_configuration)
                .AddSingleton(_socketConfig)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), interactionConfig))
                .AddSingleton<InteractionHandler>()
                .AddSingleton<CommandService>()
                .AddSingleton<HttpClient>()
                .AddSingleton<WebService>()
                .AddDbContext<BaliBotDbContext>(ServiceLifetime.Scoped)
                .AddScoped<IMessageRepository, MessageRepository>()
                .AddScoped<IReminderRepository, ReminderRepository>()
                .AddScoped<IAuthorRepository, AuthorRepository>()
                .AddScoped<IAlternativeFactRepository, AlternativeFactRepository>()
                .BuildServiceProvider();

            _scopeFactory = _services.GetRequiredService<IServiceScopeFactory>();
            MeasurementConversionHandler.GenerateAvailableMeasurementsList();
        }

        static async Task Main(string[] args)
        {
            var program = new Program();
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };
            await program.RunAsync(cts.Token);
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // Ensure database & schema exist / are upgraded
            using (var scope = _services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BaliBotDbContext>();
                db.Database.Migrate();
            }

            var client = _services.GetRequiredService<DiscordSocketClient>();

            client.Log += LogAsync;
            client.MessageReceived += MessageHandler;
            _services.GetRequiredService<CommandService>().Log += LogAsync;

            await client.LoginAsync(TokenType.Bot, _configuration["token"]);
            await client.StartAsync();

            await _services.GetRequiredService<InteractionHandler>().InitializeAsync();

            // Wait until cancellation (Ctrl+C) to gracefully stop the bot
            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // expected on shutdown
            }
            finally
            {
                await client.LogoutAsync();
                await client.StopAsync();
            }
        }

        private async Task MessageHandler(SocketMessage message)
        {
            // Ignore non-user messages and a specific username
            if (message.Author.Username == "subpixelmaster4000")
                return;
            if (message.Source != MessageSource.User)
                return;

            using var scope = _scopeFactory.CreateScope();
            var messageRepository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();

            var guild = (message.Channel as SocketGuildChannel)?.Guild;


            if (guild != null)
            {
                messageRepository.InsertMessage(message, guild);
            }

            var now = DateTime.Now;

            // Determine if April Fools behavior should be active:
            bool isTestGuild = TryGetTestGuildId(out var testGuildId) && guild?.Id == testGuildId;
            bool isConfiguredTestDate = IsAprilFoolsTestDate(now);
            bool isAprilFools = (now.Month == 4 && now.Day == 1) || (isTestGuild && isConfiguredTestDate);

            if (isAprilFools && guild != null)
            {
                // Initialize a forbidden letter per guild once per process run and announce it once
                if (!_forbiddenLettersByGuild.TryGetValue(guild.Id, out var activeLetter))
                {
                    char[] alpha = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
                    var rng = new Random();
                    activeLetter = alpha[rng.Next(alpha.Length)];
                    _forbiddenLettersByGuild[guild.Id] = activeLetter;

                    await message.Channel.SendMessageAsync($"The letter {activeLetter} cannot be used today!");
                }

                // Re-fetch letter (in case it was set above)
                activeLetter = _forbiddenLettersByGuild[guild.Id];

                if (message.Content.Contains(activeLetter, StringComparison.OrdinalIgnoreCase))
                {
                    // Try to delete the message; if we can't, quietly ignore the joke
                    var deleted = false;
                    try
                    {
                        await message.Channel.DeleteMessageAsync(message);
                        deleted = true;
                    }
                    catch
                    {
                        deleted = false;
                    }

                    if (deleted)
                    {
                        var realMessage = message.Content.Replace(activeLetter.ToString(), "*", StringComparison.OrdinalIgnoreCase);
                        var escaped = EscapeDiscordMarkdown(realMessage);
                        var displayName = message.Author.GlobalName ?? message.Author.Username;
                        await message.Channel.SendMessageAsync(
                            $"What {displayName} meant to say is \"{escaped}\"",
                            allowedMentions: AllowedMentions.None
                        );
                    }
                }
            }
            else
            {
                // Clear any stored forbidden letters when not April Fools
                _forbiddenLettersByGuild.Clear();
            }

            if (message.Content.Contains("thanks balibot", StringComparison.CurrentCultureIgnoreCase))
            {
                await message.Channel.SendMessageAsync("You're welcome!");
            }

            // filter spoilers
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

        private static string EscapeDiscordMarkdown(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            // Order matters: escape backslash first
            return input
                .Replace("\\", "\\\\")
                .Replace("*", "\\*")
                .Replace("_", "\\_")
                .Replace("~", "\\~")
                .Replace("`", "\\`")
                .Replace("|", "\\|")
                .Replace(">", "\\>");
        }

        // Try to get test guild id from configuration key "testguild"
        private bool TryGetTestGuildId(out ulong testGuildId)
        {
            testGuildId = 0;
            var raw = _configuration["testguild"];
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }
            if (ulong.TryParse(raw, out var id))
            {
                testGuildId = id;
                return true;
            }
            return false;
        }

        // Optional configuration key "aprilfools_testdate" in yyyy-MM-dd format.
        // If set, April Fools behavior will be enabled on that specific date for the test guild.
        private bool IsAprilFoolsTestDate(DateTime now)
        {
            var dateRaw = _configuration["aprilfools_testdate"];
            if (string.IsNullOrWhiteSpace(dateRaw))
            {
                // If not specified, allow forcing via boolean flag "aprilfools_alwaysOnForTestGuild".
                var alwaysOnRaw = _configuration["aprilfools_alwaysOnForTestGuild"];
                return bool.TryParse(alwaysOnRaw, out var alwaysOn) && alwaysOn;
            }
            if (DateTime.TryParse(dateRaw, out var configuredDate))
            {
                return configuredDate.Date == now.Date;
            }
            return false;
        }
    }
}