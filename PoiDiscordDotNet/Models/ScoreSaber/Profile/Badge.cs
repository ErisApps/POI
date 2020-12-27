using System.Text.Json.Serialization;

namespace PoiDiscordDotNet.Models.ScoreSaber.Profile
{
	public class Badge
	{
		[JsonPropertyName("image")]
		public string Image { get; init; } = string.Empty;

		[JsonPropertyName("description")]
		public string? Description { get; init; }
	}
}