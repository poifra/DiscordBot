using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
		var firstLocation = new PointF(1000f, 800f);
		var secondLocation = new PointF(1000f, 1050f);

		var stream = File.OpenRead("Resources/hat.png");
		var bmp = new Bitmap(stream);

		using(var graphics = Graphics.FromImage(bmp))
		{
			var format = new StringFormat()
			{
				Alignment = StringAlignment.Center,
			};
			
			using (var realFont = new Font("Crimson Text", 180, FontStyle.Bold))
			{
				graphics.DrawString(line1, realFont, Brushes.Black, firstLocation, format);
				graphics.DrawString(line2, realFont, Brushes.Black, secondLocation, format);
			}

			bmp.Save("ok.png", ImageFormat.Png);
		}
		
		//RespondWithFileAsync()
	}
}