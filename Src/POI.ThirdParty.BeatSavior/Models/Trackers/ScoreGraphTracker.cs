using System.Text.Json.Serialization;

namespace POI.ThirdParty.BeatSavior.Models.Trackers
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