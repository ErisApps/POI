using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using NodaTime;
using POI.DiscordDotNet.Services;
using Quartz;

namespace POI.DiscordDotNet.Jobs
{
	public class BirthdayGirlsJob : IJob
	{
		private const ulong DISCORD_BIRTHDAY_ROLE_ID = 728731698950307860;

		private readonly ILogger<BirthdayGirlsJob> _logger;
		private readonly UserSettingsService _userSettingsService;
		private readonly DiscordClient _discordClient;

		public BirthdayGirlsJob(ILogger<BirthdayGirlsJob> logger, UserSettingsService userSettingsService, DiscordClient discordClient)
		{
			_logger = logger;
			_userSettingsService = userSettingsService;
			_discordClient = discordClient;
		}

		public async Task Execute(IJobExecutionContext context)
		{
			var guild = await _discordClient.GetGuildAsync(561207570669371402, true).ConfigureAwait(false);
			var birthdayRole = guild.GetRole(DISCORD_BIRTHDAY_ROLE_ID);

			var localDate = LocalDate.FromDateTime(context.ScheduledFireTimeUtc.Value.LocalDateTime);
			_logger.LogInformation("Looking up birthday party people using date: {date}", localDate.ToString());
			var currentBirthdayPartyPeople = await _userSettingsService.GetAllBirthdayGirls(localDate);

			var allMembers = await guild.GetAllMembersAsync().ConfigureAwait(false);
			foreach (var member in allMembers)
			{
				var isBirthdayPartyPeep = currentBirthdayPartyPeople.Any(x => x.DiscordId == member.ToString());
				var hasBirthdayRole = member.Roles.Any(x => x.Id == DISCORD_BIRTHDAY_ROLE_ID);

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