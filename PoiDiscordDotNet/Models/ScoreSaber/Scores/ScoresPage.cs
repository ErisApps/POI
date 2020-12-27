using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PoiDiscordDotNet.Models.ScoreSaber.Scores
{
	public class ScoresPage
	{
		[JsonPropertyName("scores")]
		public List<SongScore> Scores { get; init; }
	}
}