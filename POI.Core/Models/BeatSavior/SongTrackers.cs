using System.Text.Json.Serialization;
using POI.Core.Models.BeatSavior.Trackers;

namespace POI.Core.Models.BeatSavior
{
	public class SongTrackers
	{
		[JsonPropertyName("hitTracker")]
		public HitTracker HitTracker { get; init; } = null!;

		[JsonPropertyName("accuracyTracker")]
		public AccuracyTracker AccuracyTracker { get; init; } = null!;

		[JsonPropertyName("scoreTracker")]
		public ScoreTracker ScoreTracker { get; init; } = null!;

		[JsonPropertyName("winTracker")]
		public WinTracker WinTracker { get; init; } = null!;

		[JsonPropertyName("distanceTracker")]
		public DistanceTracker DistanceTracker { get; init; } = null!;

		[JsonPropertyName("scoreGraphTracker")]
		public ScoreGraphTracker ScoreGraphTracker { get; init; } = null!;
	}
}