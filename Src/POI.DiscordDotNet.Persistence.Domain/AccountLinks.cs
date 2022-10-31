namespace POI.DiscordDotNet.Persistence.Domain
{
	public class AccountLinks
	{
		public ulong DiscordId { get; set; }
		public string? ScoreSaberId { get; set; }

		public static AccountLinks CreateDefault()
		{
			return new AccountLinks();
		}
	}
}