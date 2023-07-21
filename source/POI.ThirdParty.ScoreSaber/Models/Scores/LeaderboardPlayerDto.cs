using System.Text.Json.Serialization;
using POI.ThirdParty.ScoreSaber.Models.Shared;

namespace POI.ThirdParty.ScoreSaber.Models.Scores;

public class LeaderboardPlayerDto : PlayerInfoBaseDto
{
	[JsonPropertyName("role")]
	public string Role { get; }

	[JsonPropertyName("permissions")]
	public uint Permissions { get; }

	[JsonConstructor]
	public LeaderboardPlayerDto(string id, string name, string profilePicture, string country, string role, uint permissions)
		: base(id, name, profilePicture, country)
	{
		Role = role;
		Permissions = permissions;
	}
}