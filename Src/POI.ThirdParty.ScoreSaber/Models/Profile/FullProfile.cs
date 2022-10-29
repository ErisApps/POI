using System.Text.Json.Serialization;

namespace POI.ThirdParty.ScoreSaber.Models.Profile;

public class FullProfile : ExtendedBasicProfile
{
	[JsonPropertyName("badges")]
	public List<Badge> Badges { get; }

	[JsonConstructor]
	public FullProfile(string id, string name, string profilePicture, string country, uint rank, uint countryRank, double pp, string historyRaw, string role, uint permissions, bool inactive,
		bool banned, List<Badge> badges, ScoreStats scoreStats)
		: base(id, name, profilePicture, country, rank, countryRank, pp, historyRaw, role, permissions, inactive, banned, scoreStats)
	{
		Badges = badges;
	}
}