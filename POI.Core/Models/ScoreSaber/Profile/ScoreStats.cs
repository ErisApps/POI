using System.Text.Json.Serialization;

namespace POI.Core.Models.ScoreSaber.Profile
{
	public readonly struct ScoreStats
	{
		[JsonPropertyName("totalScore")]
		public ulong TotalScore { get; }

		[JsonPropertyName("totalRankedScore")]
		public ulong TotalRankedScore { get; }

		[JsonPropertyName("averageRankedAccuracy")]
		public double AverageRankedAccuracy { get; }

		[JsonPropertyName("totalPlayCount")]
		public uint TotalPlayCount { get; }

		[JsonPropertyName("rankedPlayCount")]
		public uint TotalRankedCount { get; }

		[JsonPropertyName("replaysWatched")]
		public uint ReplaysWatched { get; }

		[JsonConstructor]
		public ScoreStats(ulong totalScore, ulong totalRankedScore, double averageRankedAccuracy, uint totalPlayCount, uint totalRankedCount, uint replaysWatched)
		{
			TotalScore = totalScore;
			TotalRankedScore = totalRankedScore;
			AverageRankedAccuracy = averageRankedAccuracy;
			TotalPlayCount = totalPlayCount;
			TotalRankedCount = totalRankedCount;
			ReplaysWatched = replaysWatched;
		}
	}
}