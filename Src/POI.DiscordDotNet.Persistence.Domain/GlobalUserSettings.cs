using NodaTime;

namespace POI.DiscordDotNet.Persistence.Domain
{
	public class GlobalUserSettings
	{
		public ulong UserId { get; set; }

		public LocalDate? Birthday { get; set; }

		public AccountLinks AccountLinks { get; set; } = AccountLinks.CreateDefault();

		public static GlobalUserSettings CreateDefault(ulong discordId)
		{
			return new GlobalUserSettings
			{
				UserId = discordId
			};
		}
	}
}