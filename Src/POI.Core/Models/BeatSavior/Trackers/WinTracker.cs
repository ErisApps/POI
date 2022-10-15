using System.Text.Json.Serialization;

namespace POI.Core.Models.BeatSavior.Trackers
{
	public readonly struct WinTracker
	{
		[JsonPropertyName("won")]
		public bool Won { get; }

		[JsonPropertyName("rank")]
		public string Rank { get; }

		[JsonPropertyName("endTime")]
		public double EndTime { get; }

		[JsonPropertyName("nbOfPause")]
		public int NumberOfPauses { get; }

		[JsonConstructor]
		public WinTracker(bool won, string rank, double endTime, int numberOfPauses)
		{
			Won = won;
			Rank = rank;
			EndTime = endTime;
			NumberOfPauses = numberOfPauses;
		}
	}
}