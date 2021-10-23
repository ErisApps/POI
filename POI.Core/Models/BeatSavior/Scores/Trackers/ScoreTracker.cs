using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace POI.Core.Models.BeatSavior.Scores.Trackers
{
	public class ScoreTracker
	{
		[JsonPropertyName("rawScore")]
		public int RawScore { get; init; }

		[JsonPropertyName("score")]
		public int Score { get; init; }

		[JsonPropertyName("personalBest")]
		public int PersonalBest { get; init; }

		[JsonPropertyName("rawRatio")]
		public double RawRatio { get; init; }

		[JsonPropertyName("modifiedRatio")]
		public double ModifiedRatio { get; init; }

		[JsonPropertyName("personalBestRawRatio")]
		public double PersonalBestRawRatio { get; init; }

		[JsonPropertyName("personalBestModifiedRatio")]
		public double PersonalBestModifiedRatio { get; init; }

		[JsonPropertyName("modifiersMultiplier")]
		public double ModifiersMultiplier { get; init; }

		[JsonPropertyName("modifiers")]
		public List<string?> Modifiers { get; init; } = null!;
	}
}