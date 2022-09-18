using System.IO;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using POI.Core.Models.ScoreSaber.Wrappers;
using POI.Core.Services;
using POI.DiscordDotNet.Repositories;
using POI.DiscordDotNet.Services;

namespace POI.DiscordDotNet.Commands.BeatSaber
{
	[UsedImplicitly]
	public class RecentSongCommand : BaseSongCommand
	{
		public RecentSongCommand(ILogger<RecentSongCommand> logger, PathProvider pathProvider, ScoreSaberApiService scoreSaberApiService, GlobalUserSettingsRepository globalUserSettingsRepository,
			BeatSaverClientProvider beatSaverClientProvider, BeatSaviorApiService beatSaviorApiService)
			: base(logger, scoreSaberApiService, globalUserSettingsRepository, beatSaverClientProvider, Path.Combine(pathProvider.AssetsPath, "poinext1.png"),
				Path.Combine(pathProvider.AssetsPath, "Signature-Eris.png"), beatSaviorApiService)
		{
		}

		[Command("recentsong")]
		[Aliases("eris", "recentscore", "rs")]
		public async Task Handle(CommandContext ctx, [RemainingText] string _)
		{
			await GenerateScoreImageAndSendInternal(ctx);
		}

		protected override Task<PlayerScoresWrapper?> FetchScorePage(string playerId, uint page)
		{
			return ScoreSaberApiService.FetchRecentSongsScorePage(playerId, page);
		}
	}
}