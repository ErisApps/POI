using System.Text.Json.Serialization;

namespace POI.ThirdParty.BeatSavior.Models.Trackers;

public readonly struct DeepTrackersDto
{
	[JsonPropertyName("noteTracker")]
	public NoteTrackerDto NoteTracker { get; }

	[JsonConstructor]
	public DeepTrackersDto(NoteTrackerDto noteTracker)
	{
		NoteTracker = noteTracker;
	}
}