using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using POI.Core.Models.ScoreSaber.Search;
using POI.Core.Services;

namespace POI.Azure.Functions.RankUpFeed
{
	public static class RankUpFeedFunction
	{
		[Function(nameof(RankUpFeedFunction))]
		public static async Task
#if DEBUG
			<HttpResponseData>
#endif
			Run(
#if DEBUG
				[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
				HttpRequestData req
#else
			[TimerTrigger("0 */5 * * * *", RunOnStartup = false)] TimerInfo myTimer
#endif
				, FunctionContext context)
		{
			var logger = context.GetLogger(nameof(RankUpFeedFunction));

			var scoreSaberApiService = context.InstanceServices.GetService<ScoreSaberApiService>()!;

			await Task.WhenAll(
				FetchPlayers(logger, scoreSaberApiService),
				FetchRankThresholds(logger, scoreSaberApiService));

			// TODO: Post data to webhook in bot

#if DEBUG
			var response = req.CreateResponse(HttpStatusCode.OK);

			return response;
#endif
		}

		private static async Task FetchPlayers(ILogger logger, ScoreSaberApiService scoreSaberApiService)
		{
			var players = new List<SearchPlayerInfo>();
			for (var i = 0; i < 4; i++)
			{
				var internalPage = i + 1;

				logger.LogInformation("Fetching page {PageNumber} for players", internalPage);

				/*await InvokeWithFixedMinimumRunDuration(Task.Run(async () =>
				{
					var playersPage = await scoreSaberScraperService.FetchCountryLeaderboard("BE", internalPage).ConfigureAwait(false);
					if (playersPage == null)
					{
						throw new Exception();
					}

					players.AddRange(playersPage.Players);
				}), 500);*/
			}

			// TODO: Fetch linked non-BE peeps
		}

		private static async Task FetchRankThresholds(ILogger logger, ScoreSaberApiService scoreSaberApiService)
		{
			var rankThresholds = new Dictionary<int, double>();
			foreach (var (page, ranksOnPage) in new[] {25, 50, 250, 500, 2500, 5000}
				.GroupBy(rank => (int) Math.Ceiling(rank / 50f))
				.ToDictionary(kvp => kvp.Key, kvp => kvp.ToList()))
			{
				logger.LogInformation("Fetching page {PageNumber} for thresholds {Ranks}", page, string.Join(", ", ranksOnPage));

				var playersPage = await scoreSaberApiService.FetchGlobalLeaderboardsPage(page).ConfigureAwait(false);
				if (playersPage == null)
				{
					throw new Exception();
				}

				foreach (var rank in ranksOnPage)
				{
					var player = playersPage.Players.LastOrDefault(p => p.Rank == rank);
					if (player == null)
					{
						throw new Exception();
					}

					rankThresholds[rank] = player.Pp;
				}
			}
		}
	}
}