using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using POI.Core.Services;
using POI.DiscordDotNet.Services;
using Quartz;

namespace POI.DiscordDotNet.Jobs
{
	public class RankUpFeedJob : IJob
	{
		private readonly ILogger<RankUpFeedJob> _logger;
		private readonly DiscordClient _discordClient;
		private readonly ScoreSaberApiService _scoreSaberApiService;
		private readonly ScoreSaberLinkService _scoreSaberLinkService;

		private readonly SemaphoreSlim _concurrentExecutionSemaphoreSlim = new(1, 1);

		public RankUpFeedJob(ILogger<RankUpFeedJob> logger, DiscordClient discordClient, ScoreSaberApiService scoreSaberApiService, ScoreSaberLinkService scoreSaberLinkService)
		{
			_logger = logger;
			_discordClient = discordClient;
			_scoreSaberApiService = scoreSaberApiService;
			_scoreSaberLinkService = scoreSaberLinkService;
		}

		public async Task Execute(IJobExecutionContext context)
		{
			var acquiredLock = await _concurrentExecutionSemaphoreSlim.WaitAsync(0).ConfigureAwait(false);
			if (!acquiredLock)
			{
				return;
			}

			try
			{
				await ExecuteInternal(context).ConfigureAwait(false);
			}
			finally
			{
				_concurrentExecutionSemaphoreSlim.Release();
			}
		}

		private async Task ExecuteInternal(IJobExecutionContext context)
		{
			_logger.LogInformation("Heya there from the very first job");

			var allScoreSaberLinks = await _scoreSaberLinkService.GetAll().ConfigureAwait(false);
			var guild = await _discordClient.GetGuildAsync(561207570669371402, true).ConfigureAwait(false);

			var members = await guild.GetAllMembersAsync();
			if (members == null)
			{
				return;
			}

			var countryDefinition = new[] { "BE" };

			var playersWrappers = await Task.WhenAll(
				_scoreSaberApiService.FetchPlayers(1, countries: countryDefinition),
				_scoreSaberApiService.FetchPlayers(2, countries: countryDefinition),
				_scoreSaberApiService.FetchPlayers(3, countries: countryDefinition),
				_scoreSaberApiService.FetchPlayers(4, countries: countryDefinition));

			if (playersWrappers.Any(x => x == null))
			{
				return;
			}

			var roles = guild.Roles.Where(x => x.Value.Name.Contains("(Top ", StringComparison.Ordinal));

			var players = playersWrappers.SelectMany(x => x!.Players).ToList();
			foreach (var player in players)
			{
				_logger.LogDebug("#{Rank} {Name}", player.CountryRank, player.Name);

				var player1 = player;
				var discordId = allScoreSaberLinks.FirstOrDefault(x => x.ScoreSaberId == player1.Id)?.DiscordId;
				if (discordId == null)
				{
					_logger.LogWarning("Has no link");
					continue;
				}

				var member = members.FirstOrDefault(x => string.Equals(x.Id.ToString(), discordId, StringComparison.Ordinal));
				if (member == null)
				{
					_logger.LogWarning("ScoreLink exists for non-member");
					continue;
				}

				var currentTopRole = member.Roles.FirstOrDefault(x => x.Name.Contains("(Top ", StringComparison.Ordinal));
				_logger.LogDebug("Currently has role {RoleName}", currentTopRole?.Name);
			}

			Debugger.Break();
		}
	}
}