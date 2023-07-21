using System.Text.Json.Serialization;

namespace POI.ThirdParty.BeatSavior.Models.Trackers;

public readonly struct WinTrackerDto
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
	public WinTrackerDto(bool won, string rank, double endTime, int numberOfPauses)
	{
		Won = won;
		Rank = rank;
		EndTime = endTime;
		NumberOfPauses = numberOfPauses;
	}
}