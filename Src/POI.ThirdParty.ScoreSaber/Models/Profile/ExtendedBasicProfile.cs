using System.Text.Json.Serialization;

namespace POI.ThirdParty.ScoreSaber.Models.Profile
{
	public class ExtendedBasicProfile : BasicProfile
	{
		[JsonPropertyName("scoreStats")]
		public ScoreStats ScoreStats { get; }

		public ExtendedBasicProfile(string id, string name, string profilePicture, string country, uint rank, uint countryRank, double pp, string historyRaw, string role, uint permissions,
			bool inactive, bool banned, ScoreStats scoreStats)
			: base(id, name, profilePicture, country, rank, countryRank, pp, historyRaw, role, permissions, inactive, banned)
		{
			ScoreStats = scoreStats;
		}
	}
}