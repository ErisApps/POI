using System.Text.Json.Serialization;

namespace POI.ThirdParty.BeatSavior.Models.Trackers;

public readonly struct HitTracker
{
	[JsonPropertyName("leftNoteHit")]
	public int LeftNoteHit { get; }

	[JsonPropertyName("rightNoteHit")]
	public int RightNoteHit { get; }

	[JsonPropertyName("bombHit")]
	public int BombHit { get; }

	[JsonPropertyName("maxCombo")]
	public int MaxCombo { get; }

	[JsonPropertyName("nbOfWallHit")]
	public int NbOfWallHit { get; }

	[JsonPropertyName("miss")]
	public int Miss { get; }

	[JsonPropertyName("missedNotes")]
	public int MissedNotes { get; }

	[JsonPropertyName("badCuts")]
	public int BadCuts { get; }

	[JsonPropertyName("leftMiss")]
	public int LeftMiss { get; }

	[JsonPropertyName("leftBadCuts")]
	public int LeftBadCuts { get; }

	[JsonPropertyName("rightMiss")]
	public int RightMiss { get; }

	[JsonPropertyName("rightBadCuts")]
	public int RightBadCuts { get; }

	[JsonConstructor]
	public HitTracker(int leftNoteHit, int rightNoteHit, int bombHit, int maxCombo, int nbOfWallHit, int miss, int missedNotes, int badCuts, int leftMiss, int leftBadCuts, int rightMiss,
		int rightBadCuts)
	{
		LeftNoteHit = leftNoteHit;
		RightNoteHit = rightNoteHit;
		BombHit = bombHit;
		MaxCombo = maxCombo;
		NbOfWallHit = nbOfWallHit;
		Miss = miss;
		MissedNotes = missedNotes;
		BadCuts = badCuts;
		LeftMiss = leftMiss;
		LeftBadCuts = leftBadCuts;
		RightMiss = rightMiss;
		RightBadCuts = rightBadCuts;
	}
}