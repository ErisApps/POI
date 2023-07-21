namespace POI.Persistence.Models.AccountLink
{
	public sealed class ScoreSaberAccountLink : AccountLinkBase
	{
		public string ScoreSaberId { get; }

		public ScoreSaberAccountLink(ulong discordId, string scoreSaberId) : base(discordId)
		{
			ScoreSaberId = scoreSaberId;
		}
	}
}