using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using POI.Core.Models.ScoreSaber.New.Profile;
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

			await FetchPlayers(logger, scoreSaberApiService).ConfigureAwait(false);

			// TODO: Post data to webhook in bot

#if DEBUG
			var response = req.CreateResponse(HttpStatusCode.OK);

			return response;
#endif
		}

		private static async Task FetchPlayers(ILogger logger, ScoreSaberApiService scoreSaberApiService)
		{
			// Temp
			var players = new List<BasicProfile>();
			for (uint i = 0; i < 4; i++)
			{
				var internalPage = i + 1;

				logger.LogInformation("Fetching page {PageNumber} for players", internalPage);

				var playersPage = await scoreSaberApiService.FetchPlayers(internalPage, countries: new[] { "BE" }).ConfigureAwait(false);
				if (players == null)
				{
					throw new Exception();
				}

				players.AddRange(playersPage!);
			}

			// TODO: Fetch linked non-BE peeps
		}
	}
}