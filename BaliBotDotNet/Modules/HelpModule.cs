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
    }

}
