using System.Text.Json.Serialization;

namespace POI.Core.Models.ScoreSaber.Profile
{
	public class BasicProfile
	{
		[JsonPropertyName("playerInfo")]
		public ProfilePlayerInfo PlayerInfo { get; init; } = null!;
	}
}