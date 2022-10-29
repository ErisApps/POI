using System.Text.Json.Serialization;

namespace POI.ThirdParty.ScoreSaber.Models.Profile;

public class BasicProfile : ProfileBase
{
	[JsonPropertyName("role")]
	public string Role { get; }

	[JsonPropertyName("permissions")]
	public uint Permissions { get; }

	[JsonPropertyName("inactive")]
	public bool Inactive { get; }

	[JsonPropertyName("banned")]
	public bool Banned { get; }

	[JsonConstructor]
	public BasicProfile(string id, string name, string profilePicture, string country, uint rank, uint countryRank, double pp, string historyRaw, string role, uint permissions, bool inactive,
		bool banned)
		: base(id, name, profilePicture, country, rank, countryRank, pp, historyRaw)
	{
		Role = role;
		Permissions = permissions;
		Inactive = inactive;
		Banned = banned;
	}
}