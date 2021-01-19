using System.Text.Json.Serialization;

namespace PoiDiscordDotNet.Models.ScoreSaber.Profile
{
	public class FullProfile : BasicProfile
	{
		[JsonPropertyName("scoreStats")]
		public ScoreStats ScoreStats { get; init; } = null!;
	}
}