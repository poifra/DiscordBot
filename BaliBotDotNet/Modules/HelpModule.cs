using BaliBotDotNet.Services;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;

namespace BaliBotDotNet.Modules
{

    public class HelpModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
        public InteractionService Commands { get; set; }

        private InteractionHandler _handler;

        public HelpModule(InteractionHandler handler)
        {
            _handler = handler;
        }

        [SlashCommand("userinfo","Displays info on a user.")]
   
        public async Task UserInfoAsync(IUser usr = null)
        {
           // SocketGuildUser user = Context.Guild.Users.First(x => x.Id == (usr ?? Context.User).Id);
            SocketGuildUser user = (SocketGuildUser)Context.User;
            var client = new HttpClient();
            var avatar = user.GetAvatarUrl(size: 256) ?? user.GetDefaultAvatarUrl();
            Stream response = await client.GetStreamAsync(avatar);
            await ReplyAsync(user.ToString());
            await ReplyAsync($"Created on : {user.CreatedAt.ToUniversalTime()}");
            await ReplyAsync($"Joined on : {user.JoinedAt?.ToUniversalTime()}");
            await Context.Channel.SendFileAsync(response, "avatar.jpg");

        }

        [SlashCommand("help", "Lists available commands and their descriptions.")]
        public async Task ListCommandsAsync()
        {
            await DeferAsync();
            var userLocale = Context.Interaction.UserLocale?.ToLowerInvariant();
            var locale = GetSupportedLocale(userLocale);
            var localization = LoadLocalization(locale);

            var lines = new List<string>();
            foreach (var cmd in Commands.SlashCommands)
            {
                var key = cmd.Name.ToLowerInvariant();
                var (name, description) = GetLocalizedCommand(localization, key, cmd.Name, cmd.Description);
                lines.Add($"/{name} - {description}");
            }

            if (lines.Count == 0)
            {
                await FollowupAsync("No commands available.");
                return;
            }

            // Discord has a 2000 char limit per message; chunk if needed
            var message = string.Join("\n", lines);
            if (message.Length <= 1900)
            {
                await FollowupAsync(message);
            }
            else
            {
                int start = 0;
                while (start < message.Length)
                {
                    int len = System.Math.Min(1900, message.Length - start);
                    await FollowupAsync(message.Substring(start, len));
                    start += len;
                }
            }
        }

        private static string GetSupportedLocale(string userLocale)
        {
            if (userLocale.StartsWith("fr")) return "fr";
            if (userLocale.StartsWith("de")) return "de";
            return "en";
        }

        private static Dictionary<string, CommandLocalization> LoadLocalization(string locale)
        {
            try
            {
                if (locale == "fr")
                {
                    var json = File.ReadAllText("Resources/commands.fr.json");
                    return ParseLocalization(json);
                }
                if (locale == "de")
                {
                    var json = File.ReadAllText("BaliBotDotNet/Resources/commands.de.json");
                    return ParseLocalization(json);
                }
            }
            catch
            {
                // ignore and fallback to empty
            }
            return [];
        }

        private static Dictionary<string, CommandLocalization> ParseLocalization(string json)
        {
            var dict = new Dictionary<string, CommandLocalization>();
            using var doc = JsonDocument.Parse(json);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                var name = prop.Value.TryGetProperty("name", out var n) ? n.GetString() : prop.Name;
                var description = prop.Value.TryGetProperty("description", out var d) ? d.GetString() : string.Empty;
                dict[prop.Name.ToLowerInvariant()] = new CommandLocalization
                {
                    Name = name ?? prop.Name,
                    Description = description ?? string.Empty
                };
            }
            return dict;
        }

        private static (string name, string description) GetLocalizedCommand(Dictionary<string, CommandLocalization> localization, string key, string defaultName, string defaultDescription)
        {
            if (localization.TryGetValue(key, out var loc))
            {
                return (loc.Name, string.IsNullOrWhiteSpace(loc.Description) ? defaultDescription : loc.Description);
            }
            return (defaultName, defaultDescription);
        }

        private class CommandLocalization
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }
    }

}
