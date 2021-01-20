using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using PoiDiscordDotNet.Models.ScoreSaber.Scores;
using PoiDiscordDotNet.Services;

namespace PoiDiscordDotNet.Commands.Beat_Saber
{
	public class RecentSongCommand : BaseSongCommand
	{
		public RecentSongCommand(ILogger<RecentSongCommand> logger, DiscordClient client, PathProvider pathProvider, ScoreSaberService scoreSaberService, MongoDbService mongoDbService)
			: base(logger, client, scoreSaberService, mongoDbService,Path.Combine(pathProvider.AssetsPath, "poinextbg.png"))
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