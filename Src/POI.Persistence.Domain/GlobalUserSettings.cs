using NodaTime;

namespace POI.Persistence.Domain
{
	public class GlobalUserSettings
	{
		public ulong DiscordUserId { get; set; }

		public LocalDate? Birthday { get; set; }

		public string? ScoreSaberId { get; set; }

		public GlobalUserSettings(ulong discordUserId, LocalDate? birthday = null, string? scoreSaberId = null)
		{
			DiscordUserId = discordUserId;
			Birthday = birthday;
			ScoreSaberId = scoreSaberId;
		}
	}
}