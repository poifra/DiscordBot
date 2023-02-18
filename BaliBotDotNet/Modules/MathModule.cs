using BaliBotDotNet.Services;
using BaliBotDotNet.Utilities.ExtensionMethods;
using Discord.Commands;
using Discord.Interactions;
using System.Linq;
using System.Text;
using System;
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

        public enum RemoveValueChoice
        {
            DH,
            DL,
            None
        }

        [SlashCommand("roll","Rolls some dice, usage :/roll stats or /roll 5 1d6")]
        public async Task Roll(bool isRollStats = false, string numberOfOccurences = "1", string dice = "1d6", RemoveValueChoice removeValues = RemoveValueChoice.None)
        {
            if (!isRollStats)
            {
                await DeferAsync();
            }
           
            if (isRollStats)
            {
                await Roll(false, "6", "4d6", RemoveValueChoice.DL);
                return;
            }

            int dIndex = dice.IndexOf('d');
            if (dIndex == -1 || dIndex == 0)
            {
                await FollowupAsync("No dice");
                return;
            }

            var rng = new Random();
            int numberOfDice;
            int diceMax;
            int occurences = int.TryParse(numberOfOccurences, out occurences) == false ? 1 : occurences;

            if (int.TryParse(dice.AsSpan(0, dIndex), out numberOfDice) == false
                || int.TryParse(dice.AsSpan(dIndex + 1), out diceMax) == false
                || numberOfDice <= 0
                || numberOfDice > 50
                || diceMax <= 0
                || occurences < 1
                || occurences > 10)
            {
                await FollowupAsync("No dice");
                return;
            }
            int[][] results = new int[occurences][];
            for (int occ = 0; occ < occurences; occ++)
            {
                int[] row = new int[numberOfDice];
                for (int i = 0; i < numberOfDice; i++)
                {
                    row[i] = rng.Next(1, diceMax + 1);
                }
                if (removeValues == RemoveValueChoice.DH)
                {
                    row = row.OrderBy(x => x).Take(row.Length - 1).ToArray();
                }
                if (removeValues == RemoveValueChoice.DL)
                {
                    row = row.OrderByDescending(x => x).Take(row.Length - 1).ToArray();
                }
                results[occ] = row;
            }

            StringBuilder sb = new StringBuilder();
            for (int occ = 0; occ < occurences; occ++)
            {
                sb.AppendLine($"{results[occ].Join(' ')}, sum: {results.GetRow(occ).Sum()}");
            }

            await FollowupAsync(sb.ToString());

        }
    }
}
