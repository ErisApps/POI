using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace POI.DiscordDotNet.Persistence.Domain
{
	public class GlobalUserSettings
	{
		[Key]
		public ulong DiscordId { get; set; }

		public LocalDate? Birthday { get; set; }

		public AccountLinks AccountLinks { get; set; } = AccountLinks.CreateDefault();

		public static GlobalUserSettings CreateDefault(ulong discordId)
		{
			return new GlobalUserSettings
			{
				DiscordId = discordId
			};
		}
	}
}