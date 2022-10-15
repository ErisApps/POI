using System;
using ImageMagick;
using POI.Core.Services;
using POI.DiscordDotNet.Services.Interfaces;

namespace POI.DiscordDotNet.Services
{
	public class Constants : ConstantsCore, IConstants
	{
		public override string Name { get; }
		public override Version Version { get; }

		public Constants()
		{
			Name = "POINext";
			Version = typeof(Constants).Assembly.GetName().Version!;
		}

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