using System.Text.Json.Serialization;

namespace POI.Core.Models.BeatSavior.Trackers
{
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
}