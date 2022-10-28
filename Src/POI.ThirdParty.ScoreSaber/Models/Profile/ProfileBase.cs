using System.Text.Json.Serialization;
using POI.ThirdParty.ScoreSaber.Models.Shared;

namespace POI.ThirdParty.ScoreSaber.Models.Profile
{
	public class ProfileBase : PlayerInfoBase
	{
		[JsonPropertyName("rank")]
		public uint Rank { get; }

		[JsonPropertyName("countryRank")]
		public uint CountryRank { get; }

		[JsonPropertyName("pp")]
		public double Pp { get; }

		[JsonPropertyName("history")]
		public string HistoryRaw { get; }

		[JsonIgnore]
		public List<uint> History => HistoryRaw.Split(',').Where(entry => entry != "999999").Select(uint.Parse).ToList();

		public ProfileBase(string id, string name, string profilePicture, string country, uint rank, uint countryRank, double pp, string historyRaw)
			: base(id, name, profilePicture, country)
		{
			Rank = rank;
			CountryRank = countryRank;
			Pp = pp;
			HistoryRaw = historyRaw;
		}
	}
}