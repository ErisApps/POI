using System.Text.Json.Serialization;

namespace POI.ThirdParty.BeatSavior.Models.Trackers;

public readonly struct DistanceTrackerDto
{
	[JsonPropertyName("rightSaber")]
	public double RightSaber { get; }

	[JsonPropertyName("leftSaber")]
	public double LeftSaber { get; }

	[JsonPropertyName("rightHand")]
	public double RightHand { get; }

	[JsonPropertyName("leftHand")]
	public double LeftHand { get; }

	[JsonConstructor]
	public DistanceTrackerDto(double rightSaber, double leftSaber, double rightHand, double leftHand)
	{
		RightSaber = rightSaber;
		LeftSaber = leftSaber;
		RightHand = rightHand;
		LeftHand = leftHand;
	}
}