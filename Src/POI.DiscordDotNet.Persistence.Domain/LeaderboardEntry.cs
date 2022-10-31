namespace POI.DiscordDotNet.Persistence.Domain
{
	public class LeaderboardEntry
	{
		public string ScoreSaberId { get; set; }

		public string Name { get; set; }

		public uint CountryRank { get; set; }

		public double Pp { get; set; }

		public LeaderboardEntry(string scoreSaberId, string name, uint countryRank, double pp)
		{
			ScoreSaberId = scoreSaberId;
			Name = name;
			CountryRank = countryRank;
			Pp = pp;
		}
	}
}