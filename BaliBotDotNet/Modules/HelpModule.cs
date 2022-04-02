using BaliBotDotNet.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BaliBotDotNet.Modules
{

    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;

        public HelpModule(CommandService service)
        {
            _service = service;
        }

        [Command("userinfo")]
        [Summary("Get info on a user, or the user who invoked the command if one is not specified.")]
        public async Task UserInfoAsync(IUser usr = null)
        {
            SocketGuildUser user = Context.Guild.Users.First(x => x.Id == (usr ?? Context.User).Id);
            var client = new HttpClient();
            var avatar = user.GetAvatarUrl(size: 256) ?? user.GetDefaultAvatarUrl();
            Stream response = await client.GetStreamAsync(avatar);
            await ReplyAsync(user.ToString());
            await ReplyAsync($"Created on : {user.CreatedAt.ToUniversalTime()}");
            await ReplyAsync($"Joined on : {user.JoinedAt?.ToUniversalTime()}");
            await Context.Channel.SendFileAsync(response, "avatar.jpg");

        }

        [Command("commands")]
        [Summary("Display available commands.")]
        public async Task CommandListAsync()
        {
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Description = "These are the commands you can use. For help on a specific command, use $help <command>"
            };

            foreach (var module in _service.Modules)
            {
                string description = null;
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                        description += $"{CommandHandlingService.Prefix}{cmd.Aliases.First()}\n";
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }

            await ReplyAsync("", false, builder.Build());
        }

        [Command("help")]
        [Summary("Display help on a specific command")]
        public async Task HelpAsync(string command = "")
        {

            if(command.Length == 0)
            {
                await CommandListAsync();
                return;
            }

            var result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                return;
            }
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Description = $"Here are some commands like **{command}**"
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                              $"Summary: {cmd.Summary}";
                    x.IsInline = false;
                });
            }

            await ReplyAsync("", false, builder.Build());
        }
    }

}
