using System.Text.Json.Serialization;

namespace POI.Core.Models.ScoreSaber.Profile
{
	public class BasicProfile : ProfileBase
	{
		[JsonPropertyName("role")]
		public string Role { get; }

		[JsonPropertyName("permissions")]
		public uint Permissions { get; }

		[JsonPropertyName("inactive")]
		public uint Inactive { get; }

		[JsonPropertyName("banned")]
		public uint Banned { get; }

		[JsonConstructor]
		public BasicProfile(string id, string name, string profilePicture, string country, uint rank, uint countryRank, double pp, string historyRaw, string role, uint permissions, uint inactive,
			uint banned)
			: base(id, name, profilePicture, country, rank, countryRank, pp, historyRaw)
		{
			Role = role;
			Permissions = permissions;
			Inactive = inactive;
			Banned = banned;
		}
	}
}