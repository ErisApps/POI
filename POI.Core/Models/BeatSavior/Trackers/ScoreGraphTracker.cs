using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace POI.Core.Models.BeatSavior.Trackers
{
	public readonly struct ScoreGraphTracker
	{
		[JsonPropertyName("graph")]
		public Dictionary<int, double> Graph { get; }

		[JsonConstructor]
		public ScoreGraphTracker(Dictionary<int, double> graph)
		{
			Graph = graph;
		}
	}
}