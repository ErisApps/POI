using NodaTime;

namespace POI.Persistence.Domain
{
	public class GlobalUserSettings
	{
		public ulong UserId { get; set; }

		public LocalDate? Birthday { get; set; }

		public string? ScoreSaberId { get; set; }

		public GlobalUserSettings(ulong userId, LocalDate? birthday, string? scoreSaberId)
		{
			UserId = userId;
			Birthday = birthday;
			ScoreSaberId = scoreSaberId;
		}

		public GlobalUserSettings(ulong userId, LocalDate? birthday)
		{
			UserId = userId;
			Birthday = birthday;
		}

		public GlobalUserSettings(ulong userId, string? scoreSaberId)
		{
			UserId = userId;
			ScoreSaberId = scoreSaberId;
		}
	}
}