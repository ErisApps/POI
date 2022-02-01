using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using POI.Core.Models.ScoreSaber.Profile;
using POI.Core.Services;
using POI.DiscordDotNet.Extensions;
using POI.DiscordDotNet.Models.Database;
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
		private readonly MongoDbService _mongoDbService;

		private readonly SemaphoreSlim _concurrentExecutionSemaphoreSlim = new(1, 1);

		public RankUpFeedJob(ILogger<RankUpFeedJob> logger, DiscordClient discordClient, ScoreSaberApiService scoreSaberApiService, ScoreSaberLinkService scoreSaberLinkService,
			MongoDbService mongoDbService)
		{
			_logger = logger;
			_discordClient = discordClient;
			_scoreSaberApiService = scoreSaberApiService;
			_scoreSaberLinkService = scoreSaberLinkService;
			_mongoDbService = mongoDbService;
		}

		public async Task Execute(IJobExecutionContext context)
		{
			var acquiredLock = await _concurrentExecutionSemaphoreSlim.WaitAsync(0).ConfigureAwait(false);
			if (!acquiredLock)
			{
				_logger.LogWarning("Couldn't acquire lock. Job is still running...");
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
			_logger.LogInformation("Executing RankUpFeed logic");

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

			var roles = OrderTopRoles(guild.Roles.Where(x => x.Value.Name.Contains("(Top ", StringComparison.Ordinal)));

			var players = playersWrappers.SelectMany(x => x!.Players).ToList();
			foreach (var player in players)
			{
				_logger.LogDebug("#{Rank} {Name}", player.CountryRank, player.Name);

				var discordId = allScoreSaberLinks.FirstOrDefault(x => x.ScoreSaberId == player.Id)?.DiscordId;
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

				var currentTopRoles = member.Roles.Where(x => x.Name.Contains("(Top ", StringComparison.Ordinal)).ToList();
				_logger.LogDebug("Currently has role {RoleName}", string.Join(", ", currentTopRoles.Select(x => x.Name)));

				var applicableRole = DetermineApplicableRole(roles, player.Rank);

				// Check whether the applicable role is granted
				if (currentTopRoles.All(role => role.Id != applicableRole.Id))
				{
					_logger.LogInformation("Granting {PlayerName} role {RoleName}", member.DisplayName, applicableRole.Name);
					await member.GrantRoleAsync(applicableRole, "RankUpFeed update").ConfigureAwait(false);
				}

				// Revoke all other top roles if needed
				foreach (var revocableRole in currentTopRoles.Where(role => role.Id != applicableRole.Id))
				{
					_logger.LogInformation("Revoking role {RoleName} for {PlayerName}", revocableRole.Name, member.DisplayName);
					await member.RevokeRoleAsync(revocableRole, "RankUpFeed update").ConfigureAwait(false);
				}
			}

			var leaderboardEntriesCollection = _mongoDbService.GetCollection<LeaderboardEntry>();
			var originalLeaderboardEntries = await (await leaderboardEntriesCollection.FindAsync(_ => true).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false);
			var rankedUpPlayers = DetermineRankedUpPlayers(originalLeaderboardEntries, players);

			var rankUpFeedChannel = guild.GetChannel(634091663526199307);
			if (rankUpFeedChannel != null)
			{
				foreach (var player in rankedUpPlayers)
				{
					var rankUpEmbed = new DiscordEmbedBuilder()
						.WithPoiColor()
						.WithTitle($"Well done, {player.Name}")
						.WithUrl($"https://scoresaber.com/u/{player.Id}")
						.WithThumbnail(player.ProfilePicture)
						.WithDescription($"{player.Name} is now rank **#{player.CountryRank}** of the BE beat saber players with a total pp of **{player.Pp}**")
						.Build();

					await rankUpFeedChannel.SendMessageAsync(rankUpEmbed).ConfigureAwait(false);
				}
			}

			_ = await leaderboardEntriesCollection.DeleteManyAsync(_ => true).ConfigureAwait(false);
			await leaderboardEntriesCollection.InsertManyAsync(players.Select(p => new LeaderboardEntry(p.Id, p.Name, p.CountryRank))).ConfigureAwait(false);
		}

		private static List<(uint? RankThreshold, DiscordRole Role)> OrderTopRoles(IEnumerable<KeyValuePair<ulong, DiscordRole>> unorderedTopRoles)
		{
			uint? ExtractRankThresholdFromRole(string role)
			{
				var startIndex = role.LastIndexOf("(Top ", StringComparison.OrdinalIgnoreCase) + 5;
				var rankThreshold = role.Substring(startIndex, role.LastIndexOf(')') - startIndex);
				return uint.TryParse(rankThreshold, out var parsedRankedThreshold) ? parsedRankedThreshold : null;
			}

			return unorderedTopRoles
				.Select(x => (RankThreshold: ExtractRankThresholdFromRole(x.Value.Name), Role: x.Value))
				.OrderByDescending(x => x.RankThreshold ?? uint.MaxValue)
				.ToList();
		}

		private static DiscordRole DetermineApplicableRole(IReadOnlyCollection<(uint? RankThreshold, DiscordRole Role)> possibleRoles, uint rank)
		{
			var applicableRole = possibleRoles.First().Role;
			foreach (var (rankThreshold, role) in possibleRoles.Skip(1))
			{
				if (rankThreshold!.Value >= rank)
				{
					applicableRole = role;
				}
				else
				{
					break;
				}
			}

			return applicableRole;
		}

		private static List<ExtendedBasicProfile> DetermineRankedUpPlayers(IReadOnlyCollection<LeaderboardEntry> originalLeaderboard, List<ExtendedBasicProfile> currentLeaderboard)
		{
			var playersWithRankUp = new List<ExtendedBasicProfile>();
			foreach (var player in currentLeaderboard)
			{
				var oldEntry = originalLeaderboard.FirstOrDefault(x => x.ScoreSaberId == player.Id);
				if (oldEntry == null || (player.CountryRank < oldEntry.CountryRank && Math.Abs(player.Pp - oldEntry.Pp) > 0.01))
				{
					playersWithRankUp.Add(player);
				}
			}

			return playersWithRankUp;
		}
	}
}