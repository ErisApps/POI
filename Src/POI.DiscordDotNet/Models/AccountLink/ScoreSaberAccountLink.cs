namespace POI.DiscordDotNet.Models.AccountLink
{
	public class ScoreSaberAccountLink : AccountLinkBase
	{
		public string ScoreSaberId { get; }

		public ScoreSaberAccountLink(string discordId, string scoreSaberId) : base(discordId)
		{
			ScoreSaberId = scoreSaberId;
		}
	}
}