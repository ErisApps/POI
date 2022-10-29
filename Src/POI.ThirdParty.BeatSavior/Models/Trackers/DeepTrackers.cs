using System.Text.Json.Serialization;

namespace POI.ThirdParty.BeatSavior.Models.Trackers;

public readonly struct DeepTrackers
{
	[JsonPropertyName("noteTracker")]
	public NoteTracker NoteTracker { get; }

	[JsonConstructor]
	public DeepTrackers(NoteTracker noteTracker)
	{
		NoteTracker = noteTracker;
	}
}