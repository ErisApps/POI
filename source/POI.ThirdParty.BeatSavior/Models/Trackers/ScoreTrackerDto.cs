using System.Text.Json.Serialization;

namespace POI.ThirdParty.BeatSavior.Models.Trackers;

public readonly struct ScoreTrackerDto
{
	[JsonPropertyName("rawScore")]
	public int RawScore { get; }

	[JsonPropertyName("score")]
	public uint Score { get; }

	[JsonPropertyName("personalBest")]
	public int PersonalBest { get; }

	[JsonPropertyName("rawRatio")]
	public double RawRatio { get; }

	[JsonPropertyName("modifiedRatio")]
	public double ModifiedRatio { get; }

	[JsonPropertyName("personalBestRawRatio")]
	public double PersonalBestRawRatio { get; }

	[JsonPropertyName("personalBestModifiedRatio")]
	public double PersonalBestModifiedRatio { get; }

	[JsonPropertyName("modifiersMultiplier")]
	public double ModifiersMultiplier { get; }

	[JsonPropertyName("modifiers")]
	public List<string>? Modifiers { get; }

	[JsonConstructor]
	public ScoreTrackerDto(int rawScore, uint score, int personalBest, double rawRatio, double modifiedRatio, double personalBestRawRatio, double personalBestModifiedRatio,
		double modifiersMultiplier, List<string>? modifiers)
	{
		RawScore = rawScore;
		Score = score;
		PersonalBest = personalBest;
		RawRatio = rawRatio;
		ModifiedRatio = modifiedRatio;
		PersonalBestRawRatio = personalBestRawRatio;
		PersonalBestModifiedRatio = personalBestModifiedRatio;
		ModifiersMultiplier = modifiersMultiplier;
		Modifiers = modifiers;
	}
}