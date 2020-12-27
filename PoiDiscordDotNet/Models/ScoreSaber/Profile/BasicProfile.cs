using System.Text.Json.Serialization;

namespace PoiDiscordDotNet.Models.ScoreSaber.Profile
{
	public class BasicProfile
	{
		[JsonPropertyName("playerInfo")]
		public PlayerInfo PlayerInfo { get; init; }
	}
}