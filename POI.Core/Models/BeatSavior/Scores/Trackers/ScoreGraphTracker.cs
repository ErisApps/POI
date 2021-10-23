using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace POI.Core.Models.BeatSavior.Scores.Trackers
{
	public class ScoreGraphTracker
	{
		[JsonPropertyName("graph")]
		public Dictionary<int, double> Graph { get; init; }
	}
}