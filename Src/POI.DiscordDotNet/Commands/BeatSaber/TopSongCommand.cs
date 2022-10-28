using System.IO;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using POI.DiscordDotNet.Repositories;
using POI.DiscordDotNet.Services;
using POI.ThirdParty.BeatSaver.Services;
using POI.ThirdParty.BeatSavior.Services;
using POI.ThirdParty.ScoreSaber.Models.Wrappers;
using POI.ThirdParty.ScoreSaber.Services;

namespace POI.DiscordDotNet.Commands.BeatSaber
{
	[UsedImplicitly]
	public class TopSongCommand : BaseSongCommand
	{
		public TopSongCommand(ILogger<TopSongCommand> logger, PathProvider pathProvider, IScoreSaberApiService scoreSaberApiService, GlobalUserSettingsRepository globalUserSettingsRepository,
			IBeatSaverClientProvider beatSaverClientProvider, IBeatSaviorApiService beatSaviorApiService)
			: base(logger, scoreSaberApiService, globalUserSettingsRepository, beatSaverClientProvider, Path.Combine(pathProvider.AssetsPath, "poinext1.png"),
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