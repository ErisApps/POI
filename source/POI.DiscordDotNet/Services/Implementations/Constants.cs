using ImageMagick;
using POI.ThirdParty.Core.Services;

namespace POI.DiscordDotNet.Services.Implementations
{
	public class Constants : IConstants
	{
		public string Name => "POINext";
		public Version Version { get; } = typeof(Constants).Assembly.GetName().Version!;

		public static class DifficultyColors
		{
			internal static readonly MagickColor Easy = new("#4caf50");
			internal static readonly MagickColor Normal = new("#00bcd4");
			internal static readonly MagickColor Hard = new("#ff8f00");
			internal static readonly MagickColor Expert = new("#c62828");
			internal static readonly MagickColor ExpertPlus = new("#ab47bc");
		}
	}
}