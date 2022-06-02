using BaliBotDotNet.Services;
using Discord.Interactions;
using System.Threading.Tasks;

namespace BaliBotDotNet.Modules
{
    public class MathModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; set; }

        private InteractionHandler _handler;

        public MathModule(InteractionHandler handler)
        {
            _handler = handler;
        }

        [SlashCommand("gcd", "Returns the GCD of two numbers.")]
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
