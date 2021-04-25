using System.Text.Json.Serialization;

namespace POI.Core.Models.ScoreSaber.Shared
{
	public class PlayerInfoBase
	{
		[JsonPropertyName("playerId")]
		public string PlayerId { get; init; } = string.Empty;

		[JsonPropertyName("playerName")]
		public string Name { get; init; } = string.Empty;

		[JsonPropertyName("avatar")]
		public string Avatar { get; init; } = string.Empty;

		[JsonPropertyName("rank")]
		public int Rank { get; init; }

		[JsonPropertyName("pp")]
		public double Pp { get; init; }

		[JsonPropertyName("country")]
		public string Country { get; init; } = string.Empty;

		[JsonPropertyName("history")]
		public string History { get; init; } = string.Empty;
	}
}