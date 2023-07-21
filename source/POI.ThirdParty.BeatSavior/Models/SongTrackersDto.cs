using System.Text.Json.Serialization;
using POI.ThirdParty.BeatSavior.Models.Trackers;

namespace POI.ThirdParty.BeatSavior.Models;

public readonly struct SongTrackersDto
{
	[JsonPropertyName("hitTracker")]
	public HitTrackerDto HitTracker { get; }

	[JsonPropertyName("accuracyTracker")]
	public AccuracyTrackerDto AccuracyTracker { get; }

	[JsonPropertyName("scoreTracker")]
	public ScoreTrackerDto ScoreTracker { get; }

	[JsonPropertyName("winTracker")]
	public WinTrackerDto WinTracker { get; }

	[JsonPropertyName("distanceTracker")]
	public DistanceTrackerDto DistanceTracker { get; }

	[JsonPropertyName("scoreGraphTracker")]
	public ScoreGraphTrackerDto ScoreGraphTracker { get; }

	[JsonConstructor]
	public SongTrackersDto(HitTrackerDto hitTracker, AccuracyTrackerDto accuracyTracker, ScoreTrackerDto scoreTracker, WinTrackerDto winTracker, DistanceTrackerDto distanceTracker,
		ScoreGraphTrackerDto scoreGraphTracker)
	{
		HitTracker = hitTracker;
		AccuracyTracker = accuracyTracker;
		ScoreTracker = scoreTracker;
		WinTracker = winTracker;
		DistanceTracker = distanceTracker;
		ScoreGraphTracker = scoreGraphTracker;
	}
}