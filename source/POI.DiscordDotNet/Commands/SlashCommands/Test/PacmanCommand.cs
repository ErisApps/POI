using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using JetBrains.Annotations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace POI.DiscordDotNet.Commands.SlashCommands.Test
{
	public class PacmanCommand : TestSlashCommandsModule
	{
		[UsedImplicitly]
		[SlashCommand("pacman", "Just a generic command that can be used for testing ImageSharp stoofs 😅")]
		public async Task Handle(InteractionContext ctx,
			[Option("bgColor", "Color of the background (hex)")]
			string bgColorRaw,
			[Option("pacmanColor", "Color of the pacman body (hex)")]
			string pacmanColorRaw,
			[Option("pacmanEyeColor", "Color of the eye gif (hex)")]
			string pacmanEyeColorRaw)
		{
			if (!Color.TryParseHex(bgColorRaw, out var bgColor))
			{
				await ctx.CreateResponseAsync("The \"bgColor\" parameter wasn't a valid color. Please try again.", true).ConfigureAwait(false);
				return;
			}

			if (!Color.TryParseHex(pacmanColorRaw, out var pacmanColor))
			{
				await ctx.CreateResponseAsync("The \"pacmanColor\" parameter wasn't a valid color. Please try again.", true).ConfigureAwait(false);
				return;
			}

			if (!Color.TryParseHex(pacmanEyeColorRaw, out var pacmanEyeColor))
			{
				await ctx.CreateResponseAsync("The \"pacmanEyeColor\" parameter wasn't a valid color. Please try again.", true).ConfigureAwait(false);
				return;
			}

			await ctx.DeferAsync().ConfigureAwait(false);

			await using var pacmanGifStream = TestImageSharpGifStoofs(bgColor, pacmanColor, pacmanEyeColor);
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddFile("pacman.gif", pacmanGifStream)).ConfigureAwait(false);
		}

		private static Stream TestImageSharpGifStoofs(Color bgColor, Color pacmanColor, Color pacmanEyeColor)
		{
			using var gif = new Image<Rgba32>(100, 100);

			var gifMetaData = gif.Metadata.GetGifMetadata();
			gifMetaData.RepeatCount = 0;

			CreatePacManAnimationFrame(gif, 0);

			ConfigureFrameDelay(gif);
			for (var i = 1; i < 100; i++)
			{
				using var frame = new Image<Rgba32>(100, 100);
				CreatePacManAnimationFrame(frame, i);

				ConfigureFrameDelay(frame);

				gif.Frames.AddFrame(frame.Frames.RootFrame);
			}

			void ConfigureFrameDelay(Image<Rgba32> image)
			{
				const int frameDelay = 1;

				var gifFrameMetadata = image.Frames.RootFrame.Metadata.GetGifMetadata();
				gifFrameMetadata.FrameDelay = frameDelay;
			}

			void CreatePacManAnimationFrame(Image<Rgba32> image, int step)
			{
				// Background
				image.Mutate(context => context.Fill(bgColor));

				// PacMan
				var angleOffset = 22.5 * Math.Sin(Math.PI * step / 6) + 22.5;
				var path = new PathBuilder()
					.AddArc(new Point(50, 50), 35, 35, 0, (int) Math.Round(315 + angleOffset), (int) Math.Round(-270 - 2 * angleOffset))
					.LineTo(50, 50)
					.CloseFigure()
					.Build();
				image.Mutate(context => context.Fill(pacmanColor, path));

				// Eye
				var ellipsePolygon = new EllipsePolygon(new PointF(45, 35), 4);
				image.Mutate(context => context.Fill(pacmanEyeColor, ellipsePolygon));
			}

			var ms = new MemoryStream();
			gif.SaveAsGif(ms);
			ms.Seek(0, SeekOrigin.Begin);

			return ms;
		}
	}
}