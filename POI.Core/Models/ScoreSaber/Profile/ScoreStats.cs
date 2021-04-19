using System.Text.Json.Serialization;

namespace POI.Core.Models.ScoreSaber.Profile
{
	public class ScoreStats
	{
		[JsonPropertyName("totalScore")]
		public int TotalScore { get; init; }

		[JsonPropertyName("totalRankedScore")]
		public int TotalRankedScore { get; init; }

		[JsonPropertyName("averageRankedAccuracy")]
		public double AverageRankedAccuracy { get; init; }

		[JsonPropertyName("totalPlayCount")]
		public int TotalPlayCount { get; init; }

		[JsonPropertyName("rankedPlayCount")]
		public int TotalRankedCount { get; init; }
	}
}