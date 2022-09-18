using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NodaTime;
using POI.DiscordDotNet.Models.Database;
using POI.DiscordDotNet.Repositories;
using Quartz;

namespace POI.DiscordDotNet.Jobs
{
	[UsedImplicitly]
	public class BirthdayGirlsJob : IJob
	{
		private readonly ILogger<BirthdayGirlsJob> _logger;
		private readonly GlobalUserSettingsRepository _globalUserSettingsRepository;
		private readonly ServerSettingsRepository _serverSettingsRepository;
		private readonly DiscordClient _discordClient;

		public BirthdayGirlsJob(ILogger<BirthdayGirlsJob> logger, GlobalUserSettingsRepository globalUserSettingsRepository, ServerSettingsRepository serverSettingsRepository,
			DiscordClient discordClient)
		{
			_logger = logger;
			_globalUserSettingsRepository = globalUserSettingsRepository;
			_serverSettingsRepository = serverSettingsRepository;
			_discordClient = discordClient;
		}

		public async Task Execute(IJobExecutionContext context)
		{
			var localDate = LocalDate.FromDateTime(context.ScheduledFireTimeUtc?.LocalDateTime ?? DateTime.Today);
			_logger.LogInformation("Looking up birthday party people using date: {Date}", localDate.ToString());
			var currentBirthdayPartyPeople = await _globalUserSettingsRepository.GetAllBirthdayGirls(localDate);

			foreach (var (serverId, server) in _discordClient.Guilds)
			{
				await HandleServer(serverId, server, currentBirthdayPartyPeople);
			}
		}

		private async Task HandleServer(ulong serverId, DiscordGuild server, IReadOnlyCollection<GlobalUserSettings> currentBirthdayPartyPeople)
		{
			var serverSettings = await _serverSettingsRepository.FindOneById(serverId);
			if (serverSettings == null)
			{
				_logger.LogWarning("Server settings not found for server {ServerName} ({ServerId})", server.Name, serverId);
				return;
			}

			if (serverSettings.BirthdayRoleId == null)
			{
				_logger.LogWarning("Birthday role not set for server {ServerName} ({ServerId})", server.Name, serverId);
				return;
			}

			var birthdayRole = server.GetRole(serverSettings.BirthdayRoleId.Value);

			var allMembers = await server.GetAllMembersAsync().ConfigureAwait(false);
			foreach (var member in allMembers)
			{
				var isBirthdayPartyPeep = currentBirthdayPartyPeople.Any(x => x.DiscordId == member.Id.ToString());
				var hasBirthdayRole = member.Roles.Any(x => x.Id == birthdayRole.Id);

				if (isBirthdayPartyPeep && !hasBirthdayRole)
				{
					await member.GrantRoleAsync(birthdayRole, "Happy birthday ^^").ConfigureAwait(false);
				}
				else if (!isBirthdayPartyPeep && hasBirthdayRole)
				{
					await member.RevokeRoleAsync(birthdayRole, "Awww, birthday is over...").ConfigureAwait(false);
				}
			}
		}
	}
}