using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using PoiDiscordDotNet.Models.ScoreSaber.Profile;
using PoiDiscordDotNet.Models.ScoreSaber.Scores;
using PoiDiscordDotNet.Models.ScoreSaber.Search;
using Polly;
using Polly.Bulkhead;
using Polly.Retry;
using Polly.Wrap;

namespace PoiDiscordDotNet.Services
{
	public class ScoreSaberService
	{
		private const string SCORESABER_BASEURL = "https://new.scoresaber.com";
		private const string SCORESABER_API_BASEURL = SCORESABER_BASEURL + "/api/";
		private const int MAX_BULKHEAD_QUEUE_SIZE = 1000;

		private readonly ILogger<ScoreSaberService> _logger;
		private readonly HttpClient _scoreSaberApiClient;

		private readonly AsyncPolicyWrap<HttpResponseMessage> _scoreSaberApiChainedRateLimitPolicy;
		private readonly AsyncBulkheadPolicy<HttpResponseMessage> _scoreSaberApiBulkheadPolicy;
		private readonly AsyncRetryPolicy<HttpResponseMessage> _scoreSaberApiRateLimitPolicy;
		private readonly AsyncRetryPolicy<HttpResponseMessage> _scoreSaberApiInternalServerErrorRetryPolicy;
		private readonly AsyncRetryPolicy _scoreSaberImageRetryPolicy;

		private readonly JsonSerializerOptions _jsonSerializerOptions;

		public const int PLAYS_PER_PAGE = 8; // Top / Recent songs

		public ScoreSaberService(ILogger<ScoreSaberService> logger)
		{
			_logger = logger;
			_scoreSaberApiClient = new HttpClient
			{
				BaseAddress = new Uri(SCORESABER_API_BASEURL, UriKind.Absolute),
				Timeout = TimeSpan.FromSeconds(30),
				DefaultRequestVersion = HttpVersion.Version20,
				DefaultRequestHeaders = {{"User-Agent", $"{Bootstrapper.Name}/{Bootstrapper.Version.ToString(3)}"}}
			};

			_jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) {PropertyNameCaseInsensitive = false}.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

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
						_logger.LogInformation($"Hit ScoreSaber rate limit. Retrying in {timespan:g}");

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

		internal Task<BasicProfile?> FetchBasicPlayerProfile(string scoreSaberId)
		{
			return FetchData<BasicProfile?>($"{SCORESABER_API_BASEURL}player/{scoreSaberId}/basic");
		}

		internal Task<FullProfile?> FetchFullPlayerProfile(string scoreSaberId)
		{
			return FetchData<FullProfile?>($"{SCORESABER_API_BASEURL}player/{scoreSaberId}/full");
		}

		internal Task<ScoresPage?> FetchRecentSongsScorePage(string scoreSaberId, int page)
		{
			return FetchData<ScoresPage?>($"{SCORESABER_API_BASEURL}player/{scoreSaberId}/scores/recent/{page}");
		}

		internal Task<ScoresPage?> FetchTopSongsScorePage(string scoreSaberId, int page)
		{
			return FetchData<ScoresPage?>($"{SCORESABER_API_BASEURL}player/{scoreSaberId}/scores/top/{page}");
		}

		internal Task<PlayersPage?> SearchPlayersByName(string name)
		{
			if (string.IsNullOrWhiteSpace(name) || name.Length < 4 || name.Length >= 32)
			{
				throw new ArgumentException("Please enter a player name between 3 and 32 characters! (bounds not inclusive)");
			}

			return FetchData<PlayersPage?>($"{SCORESABER_API_BASEURL}players/by-name/{name}");
		}

		internal Task<byte[]?> FetchCoverImageByHash(string songHash)
		{
			return FetchImageInternal($"{SCORESABER_API_BASEURL}static/covers/{songHash}.png");
		}

		internal Task<byte[]?> FetchPlayerAvatarByProfile(string avatarPath)
		{
			return FetchImageInternal($"{SCORESABER_BASEURL}{avatarPath}");
		}

		private async Task<T?> FetchData<T>(string url) where T : class?, new()
		{
			using var response = await _scoreSaberApiChainedRateLimitPolicy.ExecuteAsync(() => _scoreSaberApiClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead));

			if (response.IsSuccessStatusCode)
			{
				try
				{
					return await response.Content.ReadFromJsonAsync<T>(_jsonSerializerOptions);
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

			return null;
		}

		private async Task<byte[]?> FetchImageInternal(string imageUrl)
		{
			try
			{
				return await _scoreSaberImageRetryPolicy.ExecuteAsync(() => _scoreSaberApiClient.GetByteArrayAsync(imageUrl));
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