namespace POI.Persistence.Models.AccountLink
{
	public abstract class AccountLinkBase
	{
		public ulong DiscordId { get; }

		protected AccountLinkBase(ulong discordId)
		{
			DiscordId = discordId;
		}
	}
}