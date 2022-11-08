namespace POI.Persistence.Domain
{
	public class AccountLinks
	{
		public ulong DiscordId { get; set; }
		public string? ScoreSaberId { get; set; }

		public static AccountLinks CreateDefault(ulong discordId)
		{
			return new AccountLinks
			{
				DiscordId = discordId
			};
		}
	}
}