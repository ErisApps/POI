using NodaTime;

namespace POI.Persistence.Domain
{
	public class GlobalUserSettings
	{
		public ulong DiscordUserId { get; set; }

		public LocalDate? Birthday { get; set; }

		public string? ScoreSaberId { get; set; }

		public string? BeatLeaderId { get; set; }

		public string? BeatSaverId { get; set; }

		public GlobalUserSettings(ulong discordUserId, LocalDate? birthday = null, string? scoreSaberId = null, string? beatLeaderId = null, string? beatSaverId = null)
		{
			DiscordUserId = discordUserId;
			Birthday = birthday;
			ScoreSaberId = scoreSaberId;
			BeatLeaderId = beatLeaderId;
			BeatSaverId = beatSaverId;
		}
	}
}