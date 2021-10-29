using System.Text.Json.Serialization;

namespace POI.Core.Models.BeatSavior.Trackers
{
	public class DeepTrackers
	{
		[JsonPropertyName("noteTracker")]
		public NoteTracker NoteTracker { get; init; } = null!;
	}
}