using Discord.Commands;
using System.Threading.Tasks;

namespace BaliBotDotNet.Modules
{
    public class MathModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;

        public MathModule(CommandService service)
        {
            _service = service;
        }

        [Command("gcd")]
        [Summary("Returns the GCD of two numbers.")]
        public async Task GCD(int a, int b)
        {
            if (b == 0)
            {
                await ReplyAsync($"{a}");
            }
            else
            {
                await GCD(b, a % b);
            }
        }
    }
}
