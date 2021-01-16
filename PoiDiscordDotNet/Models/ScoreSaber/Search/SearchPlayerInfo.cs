using System.Text.Json.Serialization;
using PoiDiscordDotNet.Models.ScoreSaber.Shared;

namespace PoiDiscordDotNet.Models.ScoreSaber.Search
{
	public class SearchPlayerInfo : PlayerInfoBase
	{
		[JsonPropertyName("difference")]
		public int Difference { get; init; }
	}
}