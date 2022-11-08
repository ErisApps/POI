using NodaTime;

namespace POI.Persistence.Domain
{
	public class GlobalUserSettings
	{
		public ulong UserId { get; set; }

		public LocalDate? Birthday { get; set; }

		public AccountLinks AccountLinks { get; set; }

		public static GlobalUserSettings CreateDefault(ulong discordId)
		{
			return new GlobalUserSettings
			{
				UserId = discordId,
				AccountLinks = AccountLinks.CreateDefault(discordId)
			};
		}
	}
}