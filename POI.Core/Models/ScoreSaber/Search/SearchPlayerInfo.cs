using System.Text.Json.Serialization;
using POI.Core.Models.ScoreSaber.Shared;

namespace POI.Core.Models.ScoreSaber.Search
{
	public class SearchPlayerInfo : PlayerInfoBase
	{
		[JsonPropertyName("difference")]
		public int Difference { get; init; }
	}
}