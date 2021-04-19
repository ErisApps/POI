using System.Collections.Generic;
using System.Text.Json.Serialization;
using POI.Core.Models.ScoreSaber.Shared;

namespace POI.Core.Models.ScoreSaber.Profile
{
	public class ProfilePlayerInfo : PlayerInfoBase
	{
		[JsonPropertyName("countryRank")]
		public int CountryRank { get; init; }

		[JsonPropertyName("role")]
		public string Role { get; init; } = string.Empty;

		[JsonPropertyName("badges")]
		public List<Badge> Badges { get; init; } = new();

		[JsonPropertyName("permissions")]
		public int Permissions { get; init; }

		[JsonPropertyName("inactive")]
		public int Inactive { get; init; }

		[JsonPropertyName("banned")]
		public int Banned { get; init; }
	}
}