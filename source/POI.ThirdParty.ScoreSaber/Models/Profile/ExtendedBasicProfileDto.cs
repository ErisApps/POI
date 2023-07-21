using System.Text.Json.Serialization;

namespace POI.ThirdParty.ScoreSaber.Models.Profile;

public class ExtendedBasicProfileDto : BasicProfileDto
{
	[JsonPropertyName("scoreStats")]
	public ScoreStatsDto ScoreStats { get; }

	[JsonConstructor]
	public ExtendedBasicProfileDto(string id, string name, string profilePicture, string country, uint rank, uint countryRank, double pp, string historyRaw, string role, uint permissions,
		bool inactive, bool banned, ScoreStatsDto scoreStats)
		: base(id, name, profilePicture, country, rank, countryRank, pp, historyRaw, role, permissions, inactive, banned)
	{
		ScoreStats = scoreStats;
	}
}