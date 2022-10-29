using System.Text.Json.Serialization;

namespace POI.ThirdParty.ScoreSaber.Models.Profile;

public readonly struct ScoreStats
{
	[JsonPropertyName("totalScore")]
	public long TotalScore { get; }

	[JsonPropertyName("totalRankedScore")]
	public long TotalRankedScore { get; }

	[JsonPropertyName("averageRankedAccuracy")]
	public double AverageRankedAccuracy { get; }

	[JsonPropertyName("totalPlayCount")]
	public uint TotalPlayCount { get; }

	[JsonPropertyName("rankedPlayCount")]
	public uint TotalRankedCount { get; }

	[JsonPropertyName("replaysWatched")]
	public uint ReplaysWatched { get; }

	[JsonConstructor]
	public ScoreStats(long totalScore, long totalRankedScore, double averageRankedAccuracy, uint totalPlayCount, uint totalRankedCount, uint replaysWatched)
	{
		TotalScore = totalScore;
		TotalRankedScore = totalRankedScore;
		AverageRankedAccuracy = averageRankedAccuracy;
		TotalPlayCount = totalPlayCount;
		TotalRankedCount = totalRankedCount;
		ReplaysWatched = replaysWatched;
	}
}