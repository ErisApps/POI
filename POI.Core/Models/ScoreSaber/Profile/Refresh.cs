using System.Text.Json.Serialization;

namespace POI.Core.Models.ScoreSaber.Profile
{
	public class Refresh
	{
		[JsonPropertyName("result")]
		public bool Result { get; init; }
	}
}