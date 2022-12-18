using System.Text.Json.Serialization;

namespace POI.ThirdParty.BeatSavior.Models.Trackers;

public readonly struct AccuracyTrackerDto
{
	[JsonPropertyName("accRight")]
	[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
	public double AccRight { get; }

	[JsonPropertyName("accLeft")]
	[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
	public double AccLeft { get; }

	[JsonPropertyName("averageAcc")]
	[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
	public double AverageAcc { get; }

	[JsonPropertyName("leftSpeed")]
	[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
	public double LeftSpeed { get; }

	[JsonPropertyName("rightSpeed")]
	[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]

	public double RightSpeed { get; }

	[JsonPropertyName("averageSpeed")]
	[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
	public double AverageSpeed { get; }

	[JsonPropertyName("leftHighestSpeed")]
	public double LeftHighestSpeed { get; }

	[JsonPropertyName("rightHighestSpeed")]
	public double RightHighestSpeed { get; }

	[JsonPropertyName("leftPreswing")]
	[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
	public double LeftPreswing { get; }

	[JsonPropertyName("rightPreswing")]
	[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
	public double RightPreswing { get; }

	[JsonPropertyName("averagePreswing")]
	[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
	public double AveragePreswing { get; }

	[JsonPropertyName("leftPostswing")]
	[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
	public double LeftPostswing { get; }

	[JsonPropertyName("rightPostswing")]
	[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
	public double RightPostswing { get; }

	[JsonPropertyName("averagePostswing")]
	[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
	public double AveragePostswing { get; }

	[JsonPropertyName("leftTimeDependence")]
	[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
	public double LeftTimeDependence { get; }

	[JsonPropertyName("rightTimeDependence")]
	[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
	public double RightTimeDependence { get; }

	[JsonPropertyName("averageTimeDependence")]
	[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
	public double AverageTimeDependence { get; }

	[JsonPropertyName("leftAverageCut")]
	[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
	public List<double> LeftAverageCut { get; }

	[JsonPropertyName("rightAverageCut")]
	[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
	public List<double> RightAverageCut { get; }

	[JsonPropertyName("averageCut")]
	[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
	public List<double> AverageCut { get; }

	[JsonPropertyName("gridAcc")]
	[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
	public List<double> GridAcc { get; }

	[JsonPropertyName("gridCut")]
	public List<int> GridCut { get; }

	[JsonConstructor]
	public AccuracyTrackerDto(double accRight, double accLeft, double averageAcc, double leftSpeed, double rightSpeed, double averageSpeed, double leftHighestSpeed, double rightHighestSpeed,
		double leftPreswing, double rightPreswing, double averagePreswing, double leftPostswing, double rightPostswing, double averagePostswing, double leftTimeDependence,
		double rightTimeDependence, double averageTimeDependence, List<double> leftAverageCut, List<double> rightAverageCut, List<double> averageCut, List<double> gridAcc, List<int> gridCut)
	{
		AccRight = accRight;
		AccLeft = accLeft;
		AverageAcc = averageAcc;
		LeftSpeed = leftSpeed;
		RightSpeed = rightSpeed;
		AverageSpeed = averageSpeed;
		LeftHighestSpeed = leftHighestSpeed;
		RightHighestSpeed = rightHighestSpeed;
		LeftPreswing = leftPreswing;
		RightPreswing = rightPreswing;
		AveragePreswing = averagePreswing;
		LeftPostswing = leftPostswing;
		RightPostswing = rightPostswing;
		AveragePostswing = averagePostswing;
		LeftTimeDependence = leftTimeDependence;
		RightTimeDependence = rightTimeDependence;
		AverageTimeDependence = averageTimeDependence;
		LeftAverageCut = leftAverageCut;
		RightAverageCut = rightAverageCut;
		AverageCut = averageCut;
		GridAcc = gridAcc;
		GridCut = gridCut;
	}
}