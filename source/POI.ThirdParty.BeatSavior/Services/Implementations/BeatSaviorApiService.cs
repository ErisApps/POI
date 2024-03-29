using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using POI.ThirdParty.Core.Services;
using POI.ThirdParty.BeatSavior.Helpers.JSON;
using POI.ThirdParty.BeatSavior.Models;
using Polly;
using Polly.Bulkhead;
using Polly.Retry;
using Polly.Wrap;

namespace POI.ThirdParty.BeatSavior.Services.Implementations;

internal class BeatSaviorApiService : IBeatSaviorApiService
{
	private const string BEATSAVIOR_BASEURL = "https://beat-savior.herokuapp.com";
	private const string BEATSAVIOR_API_BASEURL = BEATSAVIOR_BASEURL + "/api/";
	private const int MAX_BULKHEAD_QUEUE_SIZE = 1000;

	private readonly ILogger<BeatSaviorApiService> _logger;
	private readonly HttpClient _beatSaviorApiClient;

	private readonly AsyncPolicyWrap<HttpResponseMessage> _beatSaviorApiChainedRateLimitPolicy;
	private readonly AsyncBulkheadPolicy<HttpResponseMessage> _beatSaviorApiBulkheadPolicy;
	private readonly AsyncRetryPolicy<HttpResponseMessage> _beatSaviorApiRateLimitPolicy;
	private readonly AsyncRetryPolicy<HttpResponseMessage> _beatSaviorApiInternalServerErrorRetryPolicy;

	private readonly BeatSaviorSerializerContext _beatSaviorSerializerContext;

	public BeatSaviorApiService(ILogger<BeatSaviorApiService> logger, IConstants constants)
	{
		_logger = logger;
		_beatSaviorApiClient = new HttpClient
		{
			BaseAddress = new Uri(BEATSAVIOR_API_BASEURL, UriKind.Absolute),
			Timeout = TimeSpan.FromSeconds(30),
			DefaultRequestVersion = HttpVersion.Version20,
			DefaultRequestHeaders = { { "User-Agent", $"{constants.Name}/{constants.Version.ToString(3)}" } }
		};

		var jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web).ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
		_beatSaviorSerializerContext = new BeatSaviorSerializerContext(jsonSerializerOptions);

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
					_logger.LogInformation("Hit BeatSavior rate limit. Retrying in {TimeTillReset}", timespan.ToString("g"));

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

	public Task<List<SongDataDto>?> FetchBeatSaviorPlayerData(string scoreSaberId)
	{
		return FetchData($"{BEATSAVIOR_API_BASEURL}livescores/player/{scoreSaberId}", _beatSaviorSerializerContext.ListSongDataDto);
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
		catch (JsonException ex) // Invalid JSON
		{
			_logger.LogError(ex, "Invalid JSON for call: {Url}", url);
		}

		return null;
	}
}