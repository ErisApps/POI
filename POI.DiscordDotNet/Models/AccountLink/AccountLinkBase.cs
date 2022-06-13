namespace POI.DiscordDotNet.Models.AccountLink
{
	public abstract class AccountLinkBase
	{
		public string DiscordId { get; }

		protected AccountLinkBase(string discordId)
		{
			DiscordId = discordId;
		}
	}
}