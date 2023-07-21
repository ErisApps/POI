using System.Text.Json.Serialization;

namespace POI.ThirdParty.BeatSavior.Models.Trackers;

public readonly struct ScoreGraphTrackerDto
{
	[JsonPropertyName("graph")]
	public Dictionary<int, double> Graph { get; }

	[JsonConstructor]
	public ScoreGraphTrackerDto(Dictionary<int, double> graph)
	{
		Graph = graph;
	}
}