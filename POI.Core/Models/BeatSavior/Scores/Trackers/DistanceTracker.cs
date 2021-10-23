using System.Text.Json.Serialization;

namespace POI.Core.Models.BeatSavior.Scores.Trackers
{
	public class DistanceTracker
	{
		[JsonPropertyName("rightSaber")]
		public double RightSaber { get; init; }

		[JsonPropertyName("leftSaber")]
		public double LeftSaber { get; init; }

		[JsonPropertyName("rightHand")]
		public double RightHand { get; init; }

		[JsonPropertyName("leftHand")]
		public double LeftHand { get; init; }
	}
}