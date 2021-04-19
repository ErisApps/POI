using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using POI.Core.Models.ScoreSaber.Scores;
using POI.Core.Services;
using POI.DiscordDotNet.Services;

namespace POI.DiscordDotNet.Commands.Beat_Saber
{
	public class RecentSongCommand : BaseSongCommand
	{
		public RecentSongCommand(ILogger<RecentSongCommand> logger, DiscordClient client, PathProvider pathProvider, ScoreSaberService scoreSaberService, MongoDbService mongoDbService,
			BeatSaverClientProvider beatSaverClientProvider)
			: base(logger, client, scoreSaberService, mongoDbService, beatSaverClientProvider, Path.Combine(pathProvider.AssetsPath, "poinext1.png"),
				Path.Combine(pathProvider.AssetsPath, "Signature-Eris.png"))
		{
		}

		[Command("recentsong")]
		[Aliases("eris", "recentscore", "rs")]
		public async Task Handle(CommandContext ctx, [RemainingText] string _)
		{
			await GenerateScoreImageAndSendInternal(ctx);
		}

		protected override Task<ScoresPage?> FetchScorePage(string playerId, int page)
		{
			return ScoreSaberService.FetchRecentSongsScorePage(playerId, page);
		}
	}
}