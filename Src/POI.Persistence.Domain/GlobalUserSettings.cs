using NodaTime;

namespace POI.Persistence.Domain
{
	public class GlobalUserSettings
	{
		public ulong UserId { get; set; }

		public LocalDate? Birthday { get; set; }

		public string? ScoreSaberId { get; set; }

		public GlobalUserSettings(ulong userId, LocalDate? birthday = null, string? scoreSaberId = null)
		{
			UserId = userId;
			Birthday = birthday;
			ScoreSaberId = scoreSaberId;
		}
	}
}