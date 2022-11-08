using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using POI.Core.Services;
using POI.ThirdParty.ScoreSaber.Exceptions;
using POI.ThirdParty.ScoreSaber.Helpers.JSON;
using POI.ThirdParty.ScoreSaber.Models.Profile;
using POI.ThirdParty.ScoreSaber.Models.Scores;
using POI.ThirdParty.ScoreSaber.Models.Wrappers;
using Polly;
using Polly.Bulkhead;
using Polly.Retry;
using Polly.Wrap;

namespace POI.ThirdParty.ScoreSaber.Services.Implementations;

internal class ScoreSaberApiService : IScoreSaberApiService
{
	private const string SCORESABER_BASEURL = "https://scoresaber.com";
	private const string SCORESABER_API_BASEURL = SCORESABER_BASEURL + "/api/";
	private const int MAX_BULKHEAD_QUEUE_SIZE = 1000;

	private readonly ILogger<ScoreSaberApiService> _logger;
	private readonly HttpClient _scoreSaberApiClient;

	private readonly AsyncPolicyWrap<HttpResponseMessage> _scoreSaberApiChainedRateLimitPolicy;
	private readonly AsyncBulkheadPolicy<HttpResponseMessage> _scoreSaberApiBulkheadPolicy;
	private readonly AsyncRetryPolicy<HttpResponseMessage> _scoreSaberApiRateLimitPolicy;
	private readonly AsyncRetryPolicy<HttpResponseMessage> _scoreSaberApiInternalServerErrorRetryPolicy;
	private readonly AsyncRetryPolicy _scoreSaberImageRetryPolicy;

	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly ScoreSaberSerializerContext _scoreSaberSerializerContext;

	public ScoreSaberApiService(ILogger<ScoreSaberApiService> logger, IConstants constants)
	{
		_logger = logger;
		_scoreSaberApiClient = new HttpClient
		{
			BaseAddress = new Uri(SCORESABER_API_BASEURL, UriKind.Absolute),
			Timeout = TimeSpan.FromSeconds(30),
			DefaultRequestVersion = HttpVersion.Version20,
			DefaultRequestHeaders = { { "User-Agent", $"{constants.Name}/{constants.Version.ToString(3)}" } }
		};

		_jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = false }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
		_scoreSaberSerializerContext = new ScoreSaberSerializerContext(_jsonSerializerOptions);

		_scoreSaberApiInternalServerErrorRetryPolicy = Policy
			.HandleResult<HttpResponseMessage>(resp => resp.StatusCode == HttpStatusCode.InternalServerError)
			.RetryAsync(3,
				(response, attempt, context) => _logger.LogInformation("Received Internal Server Error for {Url}, retry attempt {Attempt} / 3", response.Result?.RequestMessage?.RequestUri, attempt));

		_scoreSaberApiRateLimitPolicy = Policy
			.HandleResult<HttpResponseMessage>(resp => resp.StatusCode == HttpStatusCode.TooManyRequests)
			.WaitAndRetryAsync(
				1,
				(retryAttempt, response, _) =>
				{
					response.Result.Headers.TryGetValues("x-ratelimit-reset", out var values);
					if (values != null && long.TryParse(values.FirstOrDefault(), out var unixMillisTillReset))
					{
						return TimeSpan.FromSeconds(unixMillisTillReset - DateTimeOffset.Now.ToUnixTimeSeconds());
					}

					return TimeSpan.FromSeconds(Math.Pow(10, retryAttempt));
				},
				(_, timespan, _, _) =>
				{
					_logger.LogInformation("Hit ScoreSaber rate limit. Retrying in {TimeTillReset}", timespan.ToString("g"));

					return Task.CompletedTask;
				});

		_scoreSaberApiBulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(
			4,
			MAX_BULKHEAD_QUEUE_SIZE, // Allow calls to queue indef
			_ =>
			{
				_logger.LogWarning("Bulkhead policy rejected execution");

				return Task.CompletedTask;
			});

		_scoreSaberApiChainedRateLimitPolicy = Policy.WrapAsync(
			_scoreSaberApiBulkheadPolicy,
			_scoreSaberApiInternalServerErrorRetryPolicy,
			_scoreSaberApiRateLimitPolicy);

		_scoreSaberImageRetryPolicy = Policy
			.Handle<HttpRequestException>((exception => exception.StatusCode != HttpStatusCode.NotFound))
			.Or<TaskCanceledException>()
			.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(10));
	}

	public Task<BasicProfileDto?> FetchBasicPlayerProfile(string scoreSaberId, CancellationToken cancellationToken = default)
	{
		return FetchDataClass($"{SCORESABER_API_BASEURL}player/{scoreSaberId}/basic", _scoreSaberSerializerContext.BasicProfileDto, cancellationToken);
	}

	public Task<FullProfileDto?> FetchFullPlayerProfile(string scoreSaberId, CancellationToken cancellationToken = default)
	{
		return FetchDataClass($"{SCORESABER_API_BASEURL}player/{scoreSaberId}/full", _scoreSaberSerializerContext.FullProfileDto, cancellationToken);
	}

	public Task<PlayerScoresWrapperDto?> FetchRecentSongsScorePage(string scoreSaberId, uint page, uint? limit = null, CancellationToken cancellationToken = default)
	{
		return FetchPlayerScores(scoreSaberId, page, SortType.Recent, limit, cancellationToken);
	}

	public IAsyncEnumerable<PlayerScoreDto> FetchPlayerRecentScoresPaged(string scoreSaberId, uint? itemsPerPage = null, CancellationToken cancellationToken = default)
	{
		return FetchPlayerScoresPaged(scoreSaberId, SortType.Recent, itemsPerPage, cancellationToken);
	}

	public Task<PlayerScoresWrapperDto?> FetchTopSongsScorePage(string scoreSaberId, uint page, uint? limit = null, CancellationToken cancellationToken = default)
	{
		return FetchPlayerScores(scoreSaberId, page, SortType.Top, limit, cancellationToken);
	}

	public IAsyncEnumerable<PlayerScoreDto> FetchPlayerTopScoresPaged(string scoreSaberId, uint? itemsPerPage = null, CancellationToken cancellationToken = default)
	{
		return FetchPlayerScoresPaged(scoreSaberId, SortType.Top, itemsPerPage, cancellationToken);
	}

	public Task<PlayerScoresWrapperDto?> FetchPlayerScores(string scoreSaberId, uint page, SortType sortType, uint? limit = null, CancellationToken cancellationToken = default)
	{
		var urlBuilder = new StringBuilder(SCORESABER_API_BASEURL + "player/" + scoreSaberId + "/scores?page=" + page + "&sort=" + sortType.ToString("G").ToLower());
		if (limit != null)
		{
			if (limit > 100)
			{
				throw new QueryParameterValidationException(nameof(limit));
			}

			urlBuilder.Append("&limit=").Append(limit);
		}

		return FetchDataClass(urlBuilder.ToString(), _scoreSaberSerializerContext.PlayerScoresWrapperDto, cancellationToken);
	}

	public async IAsyncEnumerable<PlayerScoreDto> FetchPlayerScoresPaged(string scoreSaberId, SortType sortType, uint? itemsPerPage = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		uint returnedPlayerCount = 0;
		uint totalPlayerCount;

		uint pagesReturned = 0;

		do
		{
			var playerScoresWrapper = await FetchPlayerScores(scoreSaberId, ++pagesReturned, sortType, itemsPerPage, cancellationToken);
			if (playerScoresWrapper == null)
			{
				_logger.LogWarning("Something went wrong :c");
				yield break;
			}

			returnedPlayerCount += (uint) playerScoresWrapper.PlayerScores.Count;
			totalPlayerCount = playerScoresWrapper.MetaData.Total;

			foreach (var score in playerScoresWrapper.PlayerScores)
			{
				yield return score;
			}
		} while (returnedPlayerCount < totalPlayerCount);
	}

	public Task<PlayersWrapperDto?> FetchPlayers(uint page, string? searchQuery = null, string[]? countries = null, CancellationToken cancellationToken = default)
	{
		var urlBuilder = new StringBuilder(SCORESABER_API_BASEURL + "players?page=" + page);
		if (searchQuery != null)
		{
			VerifySearchQueryParamWithinBounds(searchQuery);
			urlBuilder.Append("&search=").Append(searchQuery);
		}

		if (countries is { Length: > 0 })
		{
			urlBuilder.Append("&countries=").Append(string.Join(',', countries));
		}

		return FetchDataClass(urlBuilder.ToString(), _scoreSaberSerializerContext.PlayersWrapperDto, cancellationToken);
	}

	public async IAsyncEnumerable<ExtendedBasicProfileDto> FetchPlayersPaged(string? searchQuery = null, string[]? countries = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		uint returnedPlayerCount = 0;
		uint totalPlayerCount;

		uint pagesReturned = 0;

		do
		{
			var playersWrapper = await FetchPlayers(++pagesReturned, searchQuery, countries, cancellationToken);
			if (playersWrapper == null)
			{
				_logger.LogWarning("Something went wrong :c");
				yield break;
			}

			returnedPlayerCount += (uint) playersWrapper.Players.Count;
			totalPlayerCount = playersWrapper.MetaData.Total;

			foreach (var player in playersWrapper.Players)
			{
				yield return player;
			}
		} while (returnedPlayerCount < totalPlayerCount);
	}

	public Task<RefreshDto?> RefreshProfile(string scoreSaberId, CancellationToken cancellationToken = default)
	{
		if (scoreSaberId.Length != 17 || !scoreSaberId.StartsWith("7"))
		{
			throw new ArgumentException("Refreshing only works with for Steam accounts");
		}

		return FetchDataStruct($"{SCORESABER_API_BASEURL}user/{scoreSaberId}/refresh", _scoreSaberSerializerContext.RefreshDto, cancellationToken);
	}

	private static void VerifySearchQueryParamWithinBounds(string query)
	{
		if (string.IsNullOrWhiteSpace(query) || query.Length is < 4 or >= 32)
		{
			throw new ArgumentException("Please enter a player name between 3 and 32 characters! (bounds not inclusive)");
		}
	}

	public async Task<byte[]?> FetchImageFromCdn(string url, CancellationToken cancellationToken = default)
	{
		try
		{
			return await _scoreSaberImageRetryPolicy.ExecuteAsync(ct => _scoreSaberApiClient.GetByteArrayAsync(url, ct), cancellationToken);
		}
		catch (Exception e)
		{
			_logger.LogError("{Exception}", e.ToString());
			return null;
		}
	}

	private async Task<TResponseDto?> FetchDataClass<TResponseDto>(string url, JsonTypeInfo<TResponseDto> jsonResponseTypeInfo, CancellationToken cancellationToken = default)
		where TResponseDto : class
	{
		return (await FetchData(url, jsonResponseTypeInfo, cancellationToken).ConfigureAwait(false)).response;
	}

	private async Task<TResponseDto?> FetchDataStruct<TResponseDto>(string url, JsonTypeInfo<TResponseDto> jsonResponseTypeInfo, CancellationToken cancellationToken = default)
		where TResponseDto : struct
	{
		var (success, response) = await FetchData(url, jsonResponseTypeInfo, cancellationToken).ConfigureAwait(false);
		return success ? response : null;
	}

	private async Task<(bool success, TResponseDto? response)> FetchData<TResponseDto>(string url, JsonTypeInfo<TResponseDto> jsonResponseTypeInfo, CancellationToken cancellationToken = default)
	{
		using var response = await _scoreSaberApiChainedRateLimitPolicy.ExecuteAsync(ct => _scoreSaberApiClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct), cancellationToken);

		if (response.IsSuccessStatusCode)
		{
			try
			{
				return (true, await response.Content.ReadFromJsonAsync(jsonResponseTypeInfo, cancellationToken).ConfigureAwait(false));
			}
			catch (NotSupportedException) // When content type is not valid
			{
				_logger.LogError("The content type is not supported");
			}
			catch (JsonException ex) // Invalid JSON
			{
				_logger.LogError(ex, "Invalid JSON for call: {Url}", url);
			}
		}

		return (false, default);
	}
}