using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PoiDiscordDotNet.Models.ScoreSaber.Profile
{
	public class PlayerInfo
	{
		[JsonPropertyName("playerId")]
		public string PlayerId { get; init; } = string.Empty;

		[JsonPropertyName("playerName")]
		public string Name { get; init; } = string.Empty;

		[JsonPropertyName("avatar")]
		public string Avatar { get; init; } = string.Empty;

		[JsonPropertyName("rank")]
		public int Rank { get; init; }

		[JsonPropertyName("countryRank")]
		public int CountryRank { get; init; }

		[JsonPropertyName("pp")]
		public double Pp { get; init; }

		[JsonPropertyName("country")]
		public string Country { get; init; } = string.Empty;

		[JsonPropertyName("role")]
		public string Role { get; init; } = string.Empty;

		[JsonPropertyName("badges")]
		public List<Badge> Badges { get; init; } = new();

		[JsonPropertyName("history")]
		public string History { get; init; } = string.Empty;

		[JsonPropertyName("permissions")]
		public int Permissions { get; init; }

		[JsonPropertyName("inactive")]
		public int Inactive { get; init; }

		[JsonPropertyName("banned")]
		public int Banned { get; init; }
	}
}