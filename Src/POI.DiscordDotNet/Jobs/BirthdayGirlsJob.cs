using DSharpPlus.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NodaTime;
using POI.Persistence.Domain;
using POI.Persistence.Repositories;
using POI.DiscordDotNet.Services;
using Quartz;

namespace POI.DiscordDotNet.Jobs
{
	[UsedImplicitly]
	public class BirthdayGirlsJob : IJob
	{
		private readonly ILogger<BirthdayGirlsJob> _logger;
		private readonly IGlobalUserSettingsRepository _globalUserSettingsRepository;
		private readonly IServerSettingsRepository _serverSettingsRepository;
		private readonly IDiscordClientProvider _discordClientProvider;

		public BirthdayGirlsJob(ILogger<BirthdayGirlsJob> logger, IGlobalUserSettingsRepository globalUserSettingsRepository, IServerSettingsRepository serverSettingsRepository,
			IDiscordClientProvider discordClientProviderProvider)
		{
			_logger = logger;
			_globalUserSettingsRepository = globalUserSettingsRepository;
			_serverSettingsRepository = serverSettingsRepository;
			_discordClientProvider = discordClientProviderProvider;
		}

		public async Task Execute(IJobExecutionContext context)
		{
			if (_discordClientProvider.Client == null)
			{
				_logger.LogWarning("Discord client is not initialized, can't execute");
				return;
			}

			var localDate = LocalDate.FromDateTime(context.ScheduledFireTimeUtc?.LocalDateTime ?? DateTime.Today);
			_logger.LogInformation("Looking up birthday party people using date: {Date}", localDate.ToString());
			var currentBirthdayPartyPeople = await _globalUserSettingsRepository.GetAllBirthdayGirls(localDate);

			foreach (var (serverId, server) in _discordClientProvider.Client.Guilds)
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
				var isBirthdayPartyPeep = currentBirthdayPartyPeople.Any(x => x.DiscordUserId == member.Id);
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