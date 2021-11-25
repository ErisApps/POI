using System.Text.Json.Serialization;
using NodaTime;

namespace POI.Core.Models.ScoreSaber.Scores
{
	public class Score
	{
		[JsonPropertyName("id")]
		public uint Id { get; }

		[JsonPropertyName("rank")]
		public uint Rank { get; }

		[JsonPropertyName("baseScore")]
		public ulong BaseScore { get; }

		[JsonPropertyName("modifiedScore")]
		public ulong ModifiedScore { get; }

		[JsonPropertyName("pp")]
		public double Pp { get; }

		[JsonPropertyName("weight")]
		public double Weight { get; }

		[JsonPropertyName("modifiers")]
		public string Modifiers { get; }

		[JsonPropertyName("multiplier")]
		public double Multiplier { get; }

		[JsonPropertyName("badCuts")]
		public uint BadCuts { get; }

		[JsonPropertyName("missedNotes")]
		public uint MissedNotes { get; }

		[JsonPropertyName("maxCombo")]
		public uint MaxCombo { get; }

		[JsonPropertyName("fullCombo")]
		public bool FullCombo { get; }

		[JsonPropertyName("hmd")]
		// ReSharper disable once InconsistentNaming
		public HMD HMD { get; }

		[JsonPropertyName("timeSet")]
		public Instant TimeSet { get; }

		[JsonPropertyName("hasReplay")]
		public bool HasReplay { get; }

		[JsonConstructor]
		public Score(uint id, uint rank, ulong baseScore, ulong modifiedScore, double pp, double weight, string modifiers, double multiplier, uint badCuts, uint missedNotes, uint maxCombo,
			bool fullCombo, HMD hmd, Instant timeSet, bool hasReplay)
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
			HMD = hmd;
			TimeSet = timeSet;
			HasReplay = hasReplay;
		}
	}
}