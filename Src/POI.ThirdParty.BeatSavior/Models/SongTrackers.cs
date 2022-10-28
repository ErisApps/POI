using System.Text.Json.Serialization;
using POI.ThirdParty.BeatSavior.Models.Trackers;

namespace POI.ThirdParty.BeatSavior.Models
{
	public readonly struct SongTrackers
	{
		[JsonPropertyName("hitTracker")]
		public HitTracker HitTracker { get; }

		[JsonPropertyName("accuracyTracker")]
		public AccuracyTracker AccuracyTracker { get; }

		[JsonPropertyName("scoreTracker")]
		public ScoreTracker ScoreTracker { get; }

		[JsonPropertyName("winTracker")]
		public WinTracker WinTracker { get; }

		[JsonPropertyName("distanceTracker")]
		public DistanceTracker DistanceTracker { get; }

		[JsonPropertyName("scoreGraphTracker")]
		public ScoreGraphTracker ScoreGraphTracker { get; }

		[JsonConstructor]
		public SongTrackers(HitTracker hitTracker, AccuracyTracker accuracyTracker, ScoreTracker scoreTracker, WinTracker winTracker, DistanceTracker distanceTracker,
			ScoreGraphTracker scoreGraphTracker)
		{
			HitTracker = hitTracker;
			AccuracyTracker = accuracyTracker;
			ScoreTracker = scoreTracker;
			WinTracker = winTracker;
			DistanceTracker = distanceTracker;
			ScoreGraphTracker = scoreGraphTracker;
		}
	}
}