using System.IO;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using POI.Core.Models.ScoreSaber.Wrappers;
using POI.Core.Services;
using POI.DiscordDotNet.Services;

namespace POI.DiscordDotNet.Commands.BeatSaber
{
	public class TopSongCommand : BaseSongCommand
	{
		public TopSongCommand(ILogger<TopSongCommand> logger, PathProvider pathProvider, ScoreSaberApiService scoreSaberApiService, UserSettingsService userSettingsService,
			BeatSaverClientProvider beatSaverClientProvider, BeatSaviorApiService beatSaviorApiService)
			: base(logger, scoreSaberApiService, userSettingsService, beatSaverClientProvider, Path.Combine(pathProvider.AssetsPath, "poinext1.png"),
				Path.Combine(pathProvider.AssetsPath, "Signature-Eris.png"), beatSaviorApiService)
		{
		}

		[Command("topsong")]
		[Aliases("topscore", "ts")]
		public async Task Handle(CommandContext ctx, [RemainingText] string _)
		{
			await GenerateScoreImageAndSendInternal(ctx).ConfigureAwait(false);
		}

		protected override Task<PlayerScoresWrapper?> FetchScorePage(string playerId, uint page)
		{
			return ScoreSaberApiService.FetchTopSongsScorePage(playerId, page);
		}
	}
}