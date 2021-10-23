using System.Text.Json.Serialization;

namespace POI.Core.Models.BeatSavior.Scores.Trackers
{
	public class HitTracker
	{
		[JsonPropertyName("leftNoteHit")]
		public int LeftNoteHit { get; init; }

		[JsonPropertyName("rightNoteHit")]
		public int RightNoteHit { get; init; }

		[JsonPropertyName("bombHit")]
		public int BombHit { get; init; }

		[JsonPropertyName("maxCombo")]
		public int MaxCombo { get; init; }

		[JsonPropertyName("nbOfWallHit")]
		public int NbOfWallHit { get; init; }

		[JsonPropertyName("miss")]
		public int Miss { get; init; }

		[JsonPropertyName("missedNotes")]
		public int MissedNotes { get; init; }

		[JsonPropertyName("badCuts")]
		public int BadCuts { get; init; }

		[JsonPropertyName("leftMiss")]
		public int LeftMiss { get; init; }

		[JsonPropertyName("leftBadCuts")]
		public int LeftBadCuts { get; init; }

		[JsonPropertyName("rightMiss")]
		public int RightMiss { get; init; }

		[JsonPropertyName("rightBadCuts")]
		public int RightBadCuts { get; init; }
	}
}