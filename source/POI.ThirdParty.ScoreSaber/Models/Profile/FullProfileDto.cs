using System.Text.Json.Serialization;

namespace POI.ThirdParty.ScoreSaber.Models.Profile;

public class FullProfileDto : ExtendedBasicProfileDto
{
	[JsonPropertyName("badges")]
	public List<BadgeDto> Badges { get; }

	[JsonConstructor]
	public FullProfileDto(string id, string name, string profilePicture, string country, uint rank, uint countryRank, double pp, string historyRaw, string role, uint permissions, bool inactive,
		bool banned, List<BadgeDto> badges, ScoreStatsDto scoreStats)
		: base(id, name, profilePicture, country, rank, countryRank, pp, historyRaw, role, permissions, inactive, banned, scoreStats)
	{
		Badges = badges;
	}
}