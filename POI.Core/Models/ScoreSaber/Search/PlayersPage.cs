using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace POI.Core.Models.ScoreSaber.Search
{
	public class PlayersPage
	{
		[JsonPropertyName("players")]
		public List<SearchPlayerInfo> Players { get; init; } = new();
	}
}