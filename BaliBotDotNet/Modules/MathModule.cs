using BaliBotDotNet.Utilities.ExtensionMethods;
using Discord.Commands;
using System;
using System.Linq;
using System.Text;
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

        [Command("roll")]
        [Summary("Rolls a number of occurences of a series of dice. Use dl or dh to drop lowest or highest values of each occurence." +
            "\nExample usages: " +
            "\n$roll 5d20" +
            "\n$roll 4 4d6 dl")]
        public async Task Roll(string numberOfOccurences = "1", string dice = "1d6", string removeValues = "")
        {
            if (numberOfOccurences == "stats")
            {
                await Roll("6", "4d6", "dl");
                return;
            }
            int dIndex = dice.IndexOf('d');
            if (dIndex == -1 || dIndex == 0)
            {
                await ReplyAsync("No dice");
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
                || occurences > 10
                || (removeValues != "dl" && removeValues != "dh" && !removeValues.IsNullOrEmpty()))
            {
                await ReplyAsync("No dice");
                return;
            }
            int[][] results = new int[occurences][];
            for (int occ = 0; occ < occurences; occ++)
            {
                int[] row = new int[numberOfDice];
                for(int i = 0; i < numberOfDice; i++)
                {
                    row[i] = rng.Next(1,diceMax+1);
                }
                if (removeValues == "dh")
                {
                    row = row.OrderBy(x=>x).Take(row.Length-1).ToArray();
                }
                if (removeValues == "dl")
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

            await ReplyAsync(sb.ToString());

        }
    }
}
