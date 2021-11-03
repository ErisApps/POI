using System.Text.Json.Serialization;

namespace POI.Core.Models.BeatSavior.Trackers
{
	public class WinTracker
	{
		[JsonPropertyName("won")]
		public bool Won { get; init; }

		[JsonPropertyName("rank")]
		public string Rank { get; init; } = string.Empty;

		[JsonPropertyName("endTime")]
		public double EndTime { get; init; }

		[JsonPropertyName("nbOfPause")]
		public int NumberOfPauses { get; init; }
	}
}