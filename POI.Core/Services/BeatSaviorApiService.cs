using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using POI.Core.Helpers.JSON;
using POI.Core.Models.BeatSavior;
using POI.Core.Services.Interfaces;
using Polly;
using Polly.Bulkhead;
using Polly.Retry;
using Polly.Wrap;

namespace POI.Core.Services
{
	public class BeatSaviorApiService
	{
		private const string BEATSAVIOR_BASEURL = "https://www.beatsavior.io";
		private const string BEATSAVIOR_API_BASEURL = BEATSAVIOR_BASEURL + "/api/";
		private const int MAX_BULKHEAD_QUEUE_SIZE = 1000;

		private readonly ILogger<BeatSaviorApiService> _logger;
		private readonly HttpClient _beatSaviorApiClient;

		private readonly AsyncPolicyWrap<HttpResponseMessage> _beatSaviorApiChainedRateLimitPolicy;
		private readonly AsyncBulkheadPolicy<HttpResponseMessage> _beatSaviorApiBulkheadPolicy;
		private readonly AsyncRetryPolicy<HttpResponseMessage> _beatSaviorApiRateLimitPolicy;
		private readonly AsyncRetryPolicy<HttpResponseMessage> _beatSaviorApiInternalServerErrorRetryPolicy;

		private readonly JsonSerializerOptions _jsonSerializerOptions;
		private readonly BeatSaviorSerializerContext _beatSaviorSerializerContext;

		public BeatSaviorApiService(ILogger<BeatSaviorApiService> logger, IConstantsCore constants)
		{
			_logger = logger;
			_beatSaviorApiClient = new HttpClient
			{
				BaseAddress = new Uri(BEATSAVIOR_API_BASEURL, UriKind.Absolute),
				Timeout = TimeSpan.FromSeconds(30),
				DefaultRequestVersion = HttpVersion.Version20,
				DefaultRequestHeaders = { { "User-Agent", $"{constants.Name}/{constants.Version.ToString(3)}" } }
			};

			_jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = false }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
			_beatSaviorSerializerContext = new BeatSaviorSerializerContext(_jsonSerializerOptions);

			_beatSaviorApiInternalServerErrorRetryPolicy = Policy
				.HandleResult<HttpResponseMessage>(resp => resp.StatusCode == HttpStatusCode.InternalServerError)
				.RetryAsync(3, (_, attempt, _) => _logger.LogInformation("Received Internal Server Error, retry attempt {Attempt} / 3", attempt));

			_beatSaviorApiRateLimitPolicy = Policy
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

			_beatSaviorApiBulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(
				1,
				MAX_BULKHEAD_QUEUE_SIZE, // Allow calls to queue indef
				_ =>
				{
					_logger.LogWarning("Bulkhead policy rejected execution");

					return Task.CompletedTask;
				});

			_beatSaviorApiChainedRateLimitPolicy = Policy.WrapAsync(
				_beatSaviorApiBulkheadPolicy,
				_beatSaviorApiInternalServerErrorRetryPolicy,
				_beatSaviorApiRateLimitPolicy);
		}

		public Task<List<SongData>?> FetchBeatSaviorPlayerData(string scoreSaberId)
		{
			return FetchData($"{BEATSAVIOR_API_BASEURL}livescores/player/{scoreSaberId}", _beatSaviorSerializerContext.ListSongData);
		}

		private async Task<TResponse?> FetchData<TResponse>(string url, JsonTypeInfo<TResponse> jsonResponseTypeInfo) where TResponse : class
		{
			using var response = await _beatSaviorApiChainedRateLimitPolicy.ExecuteAsync(() => _beatSaviorApiClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead));

			if (!response.IsSuccessStatusCode)
			{
				return null;
			}

			try
			{
				return await response.Content.ReadFromJsonAsync(jsonResponseTypeInfo);
			}
			catch (NotSupportedException) // When content type is not valid
			{
				_logger.LogError("The content type is not supported");
			}
			catch (JsonException) // Invalid JSON
			{
				_logger.LogError("Invalid JSON");
			}

			return null;
		}
	}
}