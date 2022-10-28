using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using POI.Core.Models.ScoreSaber.Profile;
using POI.Core.Services;
using POI.DiscordDotNet.Extensions;
using POI.DiscordDotNet.Models.AccountLink;
using POI.DiscordDotNet.Models.Database;
using POI.DiscordDotNet.Repositories;
using POI.DiscordDotNet.Services.Interfaces;
using Quartz;

namespace POI.DiscordDotNet.Jobs
{
	[DisallowConcurrentExecution, UsedImplicitly]
	public class RankUpFeedJob : IJob
	{
		private const int TOP = 1000;

		private readonly ILogger<RankUpFeedJob> _logger;
		private readonly DiscordClient _discordClient;
		private readonly ScoreSaberApiService _scoreSaberApiService;
		private readonly GlobalUserSettingsRepository _globalUserSettingsRepository;
		private readonly IMongoDbService _mongoDbService;

		private readonly string[] _countryDefinition = { "BE" };
		private readonly string[] _profileRefreshExclusions = Array.Empty<string>();

		public RankUpFeedJob(ILogger<RankUpFeedJob> logger, DiscordClient discordClient, ScoreSaberApiService scoreSaberApiService, GlobalUserSettingsRepository globalUserSettingsRepository,
			IMongoDbService mongoDbService)
		{
			_logger = logger;
			_discordClient = discordClient;
			_scoreSaberApiService = scoreSaberApiService;
			_globalUserSettingsRepository = globalUserSettingsRepository;
			_mongoDbService = mongoDbService;
		}

		public async Task Execute(IJobExecutionContext context)
		{
			_logger.LogInformation("Executing RankUpFeed logic");

			var allScoreSaberLinks = await _globalUserSettingsRepository.GetAllScoreSaberAccountLinks().ConfigureAwait(false);
			var guild = await _discordClient.GetGuildAsync(561207570669371402, true).ConfigureAwait(false);

			var members = await guild.GetAllMembersAsync();
			if (members == null)
			{
				return;
			}

			var playersWrappers = await Task.WhenAll(Enumerable
				.Range(1, TOP / 50)
				.Select(page => _scoreSaberApiService.FetchPlayers((uint) page, countries: _countryDefinition)));
			if (playersWrappers.Any(x => x == null))
			{
				return;
			}

			var roles = OrderTopRoles(guild.Roles.Where(x => x.Value.Name.Contains("(Top ", StringComparison.Ordinal)));
			var players = playersWrappers
				.SelectMany(wrapper => wrapper!.Players)
				.Where(player => player.Pp > 0 && player.Rank > 0)
				.ToList();
			foreach (var player in players)
			{
				await HandlePlayer(player, allScoreSaberLinks, members, roles);
			}

			var leaderboardEntriesCollection = _mongoDbService.GetCollection<LeaderboardEntry>();
			var originalLeaderboardEntries = await (await leaderboardEntriesCollection.FindAsync(_ => true).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false);
			await PostChangesOnDiscord(guild, originalLeaderboardEntries, players);

			_ = await leaderboardEntriesCollection.DeleteManyAsync(_ => true).ConfigureAwait(false);
			await leaderboardEntriesCollection.InsertManyAsync(players.Select(p => new LeaderboardEntry(p.Id, p.Name, p.CountryRank, p.Pp))).ConfigureAwait(false);
		}

		private async Task HandlePlayer(ProfileBase player,
			IEnumerable<ScoreSaberAccountLink> scoreSaberLinks,
			IEnumerable<DiscordMember> members,
			IReadOnlyCollection<(uint? RankThreshold, DiscordRole Role)> roles)
		{
			// _logger.LogDebug("#{Rank} {Name}", player.CountryRank, player.Name);
			await TriggerProfileRefreshIfNeeded(player);

			var discordId = scoreSaberLinks.FirstOrDefault(x => x.ScoreSaberId == player.Id)?.DiscordId;
			if (discordId == null)
			{
				// _logger.LogWarning("No ScoreLink found for player {PlayerName}", player.Name);
				return;
			}

			var member = members.FirstOrDefault(x => string.Equals(x.Id.ToString(), discordId, StringComparison.Ordinal));
			if (member == null)
			{
				// _logger.LogWarning("ScoreLink exists for non-Discord-member. ScoreSaber name: {PlayerName}, ScoreSaberId: {PlayerId}", player.Name, player.Id);
				return;
			}

			var currentTopRoles = member.Roles.Where(x => x.Name.Contains("(Top ", StringComparison.Ordinal)).ToList();
			// _logger.LogDebug("Currently has role {RoleName}", string.Join(", ", currentTopRoles.Select(x => x.Name)));

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

		private async Task TriggerProfileRefreshIfNeeded(ProfileBase player)
		{
			if (player.ProfilePicture.EndsWith("steam.png") && player.Id.Length == 17 && player.Id.StartsWith("7"))
			{
				if (_profileRefreshExclusions.Contains(player.Id))
				{
					_logger.LogWarning("Refresh prevented for player {PlayerName} at rank {PlayerRank} due to explicit exclusion", player.Name, player.CountryRank);
				}
				else
				{
					_logger.LogInformation("Calling Refresh for player {PlayerName} at rank {PlayerRank}", player.Name, player.CountryRank);
					await _scoreSaberApiService.RefreshProfile(player.Id).ConfigureAwait(false);
				}
			}
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

		private static async Task PostChangesOnDiscord(DiscordGuild guild, IReadOnlyCollection<LeaderboardEntry> originalLeaderboardEntries, List<ExtendedBasicProfile> players)
		{
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