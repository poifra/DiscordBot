using Discord.Interactions;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BaliBotDotNet.Modules;

public class FishHatModule: InteractionModuleBase<SocketInteractionContext>
{
	[SlashCommand("fish", "Creates an awesome hat.")]
	public async Task Fish()
	{
		var words = new List<string> { "women", "want", "me", "fish", "fear", "me" };
		words = words.Shuffle().ToList();
		var line1 = string.Join(" ", words.Take(3)) + ",";
		var line2 = string.Join(" ", words.Skip(3).Take(3)) + ".";
		var firstLocation = new PointF(64f, 64f);
		var secondLocation = new PointF(64f, 50f);

		var stream = File.OpenRead("Resources/hat.png");
		var bmp = new Bitmap(stream);

		using (var graphics = Graphics.FromImage(bmp))
		{
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            var format = new StringFormat()
			{
				Alignment = StringAlignment.Center
			};

			using (var realFont = new Font("Crimson Text", 11, FontStyle.Bold))
			{
				graphics.DrawString(line1, realFont, Brushes.Black, firstLocation, format);
				graphics.DrawString(line2, realFont, Brushes.Black, secondLocation, format);
			}

			var outputStream = new MemoryStream();
			bmp.Save(outputStream, ImageFormat.Png);

			await RespondWithFileAsync(outputStream, "hat.png");
		}
	}
}