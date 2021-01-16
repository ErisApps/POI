using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PoiDiscordDotNet.Models.ScoreSaber.Search
{
	public class PlayersPage
	{
		[JsonPropertyName("players")]
		public List<SearchPlayerInfo> Players { get; init; }
	}
}