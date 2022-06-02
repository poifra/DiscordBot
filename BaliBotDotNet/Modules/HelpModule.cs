using BaliBotDotNet.Services;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

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

        [SlashCommand("commands","Get a list of commands")]
        public async Task CommandListAsync()
        {
            //var builder = new EmbedBuilder()
            //{
            //    Color = new Color(114, 137, 218),
            //    Description = "These are the commands you can use. For help on a specific command, use $help <command>"
            //};

            //foreach (var module in Commands.Modules)
            //{
            //    string description = null;
            //    foreach (var cmd in module.CommandService.ContextCommands)
            //    {
            //        var result = await cmd.CheckPreconditionsAsync(Context);
            //        if (result.IsSuccess)
            //            description += $"{InteractionHandler.Prefix}{cmd.Aliases.First()}\n";
            //    }

            //    if (!string.IsNullOrWhiteSpace(description))
            //    {
            //        builder.AddField(x =>
            //        {
            //            x.Name = module.Name;
            //            x.Value = description;
            //            x.IsInline = false;
            //        });
            //    }
            //}

            //await ReplyAsync("", false, builder.Build());
        }

        [SlashCommand("help", "Gets help on a specific command")]
        public async Task HelpAsync(string command = "")
        {

            //if(command.Length == 0)
            //{
            //    await CommandListAsync();
            //    return;
            //}

            //var result = Commands.Search(Context, command);

            //if (!result.IsSuccess)
            //{
            //    await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
            //    return;
            //}
            //var builder = new EmbedBuilder()
            //{
            //    Color = new Color(114, 137, 218),
            //    Description = $"Here are some commands like **{command}**"
            //};

            //foreach (var match in result.Commands)
            //{
            //    var cmd = match.Command;

            //    builder.AddField(x =>
            //    {
            //        x.Name = string.Join(", ", cmd.Aliases);
            //        x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
            //                  $"Summary: {cmd.Summary}";
            //        x.IsInline = false;
            //    });
            //}

            //await ReplyAsync("", false, builder.Build());
        }
    }

}
