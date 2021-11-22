using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using POI.Core.Models.ScoreSaber.New.Scores;
using POI.Core.Services;
using POI.DiscordDotNet.Services;

namespace POI.DiscordDotNet.Commands.Beat_Saber
{
	public class RecentSongCommand : BaseSongCommand
	{
		public RecentSongCommand(ILogger<RecentSongCommand> logger, DiscordClient client, PathProvider pathProvider, ScoreSaberApiService scoreSaberApiService, MongoDbService mongoDbService,
			BeatSaverClientProvider beatSaverClientProvider, BeatSaviorApiService beatSaviorApiService)
			: base(logger, client, scoreSaberApiService, mongoDbService, beatSaverClientProvider, Path.Combine(pathProvider.AssetsPath, "poinext1.png"),
				Path.Combine(pathProvider.AssetsPath, "Signature-Eris.png"), beatSaviorApiService)
		{
		}

		[Command("recentsong")]
		[Aliases("eris", "recentscore", "rs")]
		public async Task Handle(CommandContext ctx, [RemainingText] string _)
		{
			await GenerateScoreImageAndSendInternal(ctx);
		}

		protected override Task<List<PlayerScore>?> FetchScorePage(string playerId, uint page)
		{
			return ScoreSaberApiService.FetchRecentSongsScorePage(playerId, page);
		}
	}
}