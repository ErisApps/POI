using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace POI.Core.Models.ScoreSaber.Scores
{
	public class ScoresPage
	{
		[JsonPropertyName("scores")]
		public List<SongScore> Scores { get; init; } = null!;
	}
}