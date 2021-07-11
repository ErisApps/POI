using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using Microsoft.Extensions.Logging;
using POI.Core.Models.ScoreSaber.Search;
using POI.Core.Services.Interfaces;

namespace POI.Core.Services
{
	public class ScoreSaberScraperService
	{
		private const string SCORESABER_BASEURL = "https://scoresaber.com/";

		private readonly ILogger<ScoreSaberScraperService> _logger;
		private readonly IConfiguration? _sharedConfiguration;

		private readonly CultureInfo _cultureInfo;

		public ScoreSaberScraperService(ILogger<ScoreSaberScraperService> logger, IConstantsCore constants)
		{
			_logger = logger;

			var requester = new DefaultHttpRequester();
			requester.Headers[HeaderNames.UserAgent] = $"{constants.Name}/{constants.Version.ToString(3)}";
			_sharedConfiguration = Configuration.Default.With(requester).WithDefaultLoader();

			_cultureInfo = CultureInfo.GetCultureInfo("en-US");
		}

		public async Task<PlayersPage?> FetchCountryLeaderboard(string countryCode, int page)
		{
			var document = await FetchDocumentInternal($"{SCORESABER_BASEURL}global/{page}?country={countryCode}").ConfigureAwait(false);
			if (document == null)
			{
				return null;
			}

			var playersPage = new PlayersPage();
			var leaderboardPlayers = document.QuerySelectorAll("table.ranking.global tbody tr");
			playersPage.Players.AddRange(leaderboardPlayers.Select(playerRow =>
			{
				var playerElement = playerRow.QuerySelector(".player")!;
				return new SearchPlayerInfo
				{
					PlayerId = playerElement.QuerySelector("a")!.Attributes["href"]!.Value[3..],
					Name = playerElement.QuerySelector("span")!.FirstChild?.TextContent ?? string.Empty,
					Rank = int.Parse(playerRow.QuerySelector(".rank")!.FirstChild!.TextContent.Trim()[1..], NumberStyles.Any, _cultureInfo),
					Avatar = playerRow.QuerySelector(".picture img")!.Attributes["src"]!.Value,
					Pp = double.Parse(playerRow.QuerySelector(".pp .ppValue")!.FirstChild!.TextContent, NumberStyles.Any, _cultureInfo),
					Country = countryCode.ToUpper(),
					Difference = int.Parse(playerRow.QuerySelector(".diff")!.TextContent.Trim(), NumberStyles.Any, _cultureInfo)
				};
			}));

			return playersPage;
		}

		private async Task<IDocument?> FetchDocumentInternal(string url)
		{
			try
			{
				var context = BrowsingContext.New(_sharedConfiguration);
				return await context.OpenAsync(url).ConfigureAwait(false);
			}
			catch (Exception)
			{
				_logger.LogWarning("Something went wrong while trying to fetch the page for url: {Url}", url);
				return null;
			}
		}
	}
}