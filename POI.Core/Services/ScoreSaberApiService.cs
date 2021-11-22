using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using POI.Core.Exceptions;
using POI.Core.Helpers.JSON;
using POI.Core.Models.ScoreSaber.Profile;
using POI.Core.Models.ScoreSaber.Scores;
using POI.Core.Services.Interfaces;
using Polly;
using Polly.Bulkhead;
using Polly.Retry;
using Polly.Wrap;

namespace POI.Core.Services
{
	public class ScoreSaberApiService
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

		public const int PLAYS_PER_PAGE = 8; // Top / Recent songs

		public ScoreSaberApiService(ILogger<ScoreSaberApiService> logger, IConstantsCore constants)
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
				.RetryAsync(3, (_, attempt, _) => _logger.LogInformation("Received Internal Server Error, retry attempt {Attempt} / 3", attempt));

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
				1,
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

		public Task<BasicProfile?> FetchBasicPlayerProfile(string scoreSaberId)
		{
			return FetchDataClass($"{SCORESABER_API_BASEURL}player/{scoreSaberId}/basic", _scoreSaberSerializerContext.BasicProfile);
		}

		public Task<FullProfile?> FetchFullPlayerProfile(string scoreSaberId)
		{
			return FetchDataClass($"{SCORESABER_API_BASEURL}player/{scoreSaberId}/full", _scoreSaberSerializerContext.FullProfile);
		}

		public Task<List<PlayerScore>?> FetchRecentSongsScorePage(string scoreSaberId, uint page, uint? limit = null)
		{
			return FetchPlayerScores(scoreSaberId, page, SortType.Recent, limit);
		}

		public Task<List<PlayerScore>?> FetchTopSongsScorePage(string scoreSaberId, uint page, uint? limit = null)
		{
			return FetchPlayerScores(scoreSaberId, page, SortType.Top, limit);
		}

		public Task<List<PlayerScore>?> FetchPlayerScores(string scoreSaberId, uint page, SortType sortType, uint? limit = null)
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

			return FetchDataClass(urlBuilder.ToString(), _scoreSaberSerializerContext.ListPlayerScore);
		}

		// TODO: Add intermediate model inheriting from BasicProfile that contains ScoreStats but doesn't have badges
		// using FullProfile would be violating the nullability constraint of the Badges property
		public Task<List<BasicProfile>?> FetchPlayers(uint page, string? searchQuery = null, string[]? countries = null)
		{
			var urlBuilder = new StringBuilder(SCORESABER_API_BASEURL + "players?page=" + page);
			if (searchQuery != null)
			{
				VerifySearchQueryParamWithinBounds(searchQuery);
				urlBuilder.Append("&search=").Append(searchQuery);
			}

			if (countries is { Length: > 0 })
			{
				urlBuilder.Append("?countries=").Append(string.Join(',', countries));
			}

			return FetchDataClass(urlBuilder.ToString(), _scoreSaberSerializerContext.ListBasicProfile);
		}

		public Task<Refresh?> RefreshProfile(string scoreSaberId)
		{
			if (scoreSaberId.Length != 17 || scoreSaberId.StartsWith("7"))
			{
				throw new ArgumentException("Refreshing only works with for Steam accounts");
			}

			return FetchDataStruct($"{SCORESABER_API_BASEURL}user/{scoreSaberId}/refresh", _scoreSaberSerializerContext.Refresh);
		}

		private void VerifySearchQueryParamWithinBounds(string query)
		{
			if (string.IsNullOrWhiteSpace(query) || query.Length is < 4 or >= 32)
			{
				throw new ArgumentException("Please enter a player name between 3 and 32 characters! (bounds not inclusive)");
			}
		}

		private async Task<TResponse?> FetchDataClass<TResponse>(string url, JsonTypeInfo<TResponse> jsonResponseTypeInfo) where TResponse : class
		{
			return (await FetchData(url, jsonResponseTypeInfo).ConfigureAwait(false)).response;
		}

		private async Task<TResponse?> FetchDataStruct<TResponse>(string url, JsonTypeInfo<TResponse> jsonResponseTypeInfo) where TResponse : struct
		{
			var (success, response) = await FetchData(url, jsonResponseTypeInfo).ConfigureAwait(false);
			return success ? response : null;
		}

		private async Task<(bool success, TResponse? response)> FetchData<TResponse>(string url, JsonTypeInfo<TResponse> jsonResponseTypeInfo)
		{
			using var response = await _scoreSaberApiChainedRateLimitPolicy.ExecuteAsync(() => _scoreSaberApiClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead));

			if (response.IsSuccessStatusCode)
			{
				try
				{
					return (true, await response.Content.ReadFromJsonAsync(jsonResponseTypeInfo).ConfigureAwait(false));
				}
				catch (NotSupportedException) // When content type is not valid
				{
					_logger.LogError("The content type is not supported");
				}
				catch (JsonException) // Invalid JSON
				{
					_logger.LogError("Invalid JSON");
				}
			}

			return (false, default);
		}

		public async Task<byte[]?> FetchImageFromCdn(string url)
		{
			try
			{
				return await _scoreSaberImageRetryPolicy.ExecuteAsync(() => _scoreSaberApiClient.GetByteArrayAsync(url));
			}
			catch (Exception e)
			{
				_logger.LogError("{Exception}", e.ToString());
				return null;
			}
		}

		/*private void TestRateLimitPolicy()
		{
			for (var i = 0; i < 200; i++)
			{
				_logger.LogInformation($"Fetching page {i + 1:000} of 200");
				var internalI = i;
				_scoreSaberApiChainedRateLimitPolicy.ExecuteAsync(() =>
						_scoreSaberApiClient.GetAsync(
							$"https://new.scoresaber.com/api/player/76561198333869741/scores/top/{internalI}"))
					.ContinueWith(_ => _logger.LogInformation($"Finished fetching page {internalI}\n" +
					                                          "Bulkhead stats:\n" +
					                                          $"Bulkhead queue size: {MaxBulkheadQueueSize}\n" +
					                                          $"Bulkhead queued count: {MaxBulkheadQueueSize - _scoreSaberBulkheadPolicy.QueueAvailableCount}"));
			}
		}*/
	}
}