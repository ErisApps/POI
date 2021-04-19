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
	public class TopSongCommand : BaseSongCommand
	{
		public TopSongCommand(ILogger<TopSongCommand> logger, DiscordClient client, PathProvider pathProvider, ScoreSaberService scoreSaberService, MongoDbService mongoDbService,
			BeatSaverClientProvider beatSaverClientProvider)
			: base(logger, client, scoreSaberService, mongoDbService, beatSaverClientProvider, Path.Combine(pathProvider.AssetsPath, "poinext1.png"),
				Path.Combine(pathProvider.AssetsPath, "Signature-Eris.png"))
		{
		}

		[Command("topsong")]
		[Aliases("topscore", "ts")]
		public async Task Handle(CommandContext ctx, [RemainingText] string _)
		{
			await GenerateScoreImageAndSendInternal(ctx).ConfigureAwait(false);
		}

		protected override Task<ScoresPage?> FetchScorePage(string playerId, int page)
		{
			return ScoreSaberService.FetchTopSongsScorePage(playerId, page);
		}
	}
}