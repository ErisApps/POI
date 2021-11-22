using System.Text.Json.Serialization;
using NodaTime;

namespace POI.Core.Models.ScoreSaber.New.Scores
{
	public class Score
	{
		[JsonPropertyName("id")]
		public int Id { get; }

		[JsonPropertyName("rank")]
		public int Rank { get; }

		[JsonPropertyName("baseScore")]
		public int BaseScore { get; }

		[JsonPropertyName("modifiedScore")]
		public int ModifiedScore { get; }

		[JsonPropertyName("pp")]
		public double Pp { get; }

		[JsonPropertyName("weight")]
		public double Weight { get; }

		[JsonPropertyName("modifiers")]
		public string Modifiers { get; }

		[JsonPropertyName("multiplier")]
		public int Multiplier { get; }

		[JsonPropertyName("badCuts")]
		public int BadCuts { get; }

		[JsonPropertyName("missedNotes")]
		public int MissedNotes { get; }

		[JsonPropertyName("maxCombo")]
		public int MaxCombo { get; }

		[JsonPropertyName("fullCombo")]
		public int FullCombo { get; }

		[JsonPropertyName("hmd")]
		public int Hmd { get; }

		[JsonPropertyName("timeSet")]
		public Instant TimeSet { get; }

		[JsonPropertyName("hasReplay")]
		public bool HasReplay { get; }

		[JsonConstructor]
		public Score(int id, int rank, int baseScore, int modifiedScore, double pp, double weight, string modifiers, int multiplier, int badCuts, int missedNotes, int maxCombo, int fullCombo,
			int hmd, Instant timeSet, bool hasReplay)
		{
			Id = id;
			Rank = rank;
			BaseScore = baseScore;
			ModifiedScore = modifiedScore;
			Pp = pp;
			Weight = weight;
			Modifiers = modifiers;
			Multiplier = multiplier;
			BadCuts = badCuts;
			MissedNotes = missedNotes;
			MaxCombo = maxCombo;
			FullCombo = fullCombo;
			Hmd = hmd;
			TimeSet = timeSet;
			HasReplay = hasReplay;
		}
	}
}