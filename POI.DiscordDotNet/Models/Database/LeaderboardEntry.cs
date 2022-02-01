using MongoDB.Bson.Serialization.Attributes;

namespace POI.DiscordDotNet.Models.Database
{
	public class LeaderboardEntry
	{
		[BsonId]
		public string ScoreSaberId { get; }

		public string Name { get; }

		public uint CountryRank { get; }

		public double Pp { get; }

		[BsonConstructor]
		public LeaderboardEntry(string scoreSaberId, string name, uint countryRank, double pp)
		{
			ScoreSaberId = scoreSaberId;
			Name = name;
			CountryRank = countryRank;
			Pp = pp;
		}
	}
}