using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace POI.Core.Models.BeatSavior.Trackers
{
	public readonly struct AccuracyTracker
	{
		[JsonPropertyName("accRight")]
		public double AccRight { get; }

		[JsonPropertyName("accLeft")]
		public double AccLeft { get; }

		[JsonPropertyName("averageAcc")]
		public double AverageAcc { get; }

		[JsonPropertyName("leftSpeed")]
		public double LeftSpeed { get; }

		[JsonPropertyName("rightSpeed")]
		public double RightSpeed { get; }

		[JsonPropertyName("averageSpeed")]
		public double AverageSpeed { get; }

		[JsonPropertyName("leftHighestSpeed")]
		public double LeftHighestSpeed { get; }

		[JsonPropertyName("rightHighestSpeed")]
		public double RightHighestSpeed { get; }

		[JsonPropertyName("leftPreswing")]
		public double LeftPreswing { get; }

		[JsonPropertyName("rightPreswing")]
		public double RightPreswing { get; }

		[JsonPropertyName("averagePreswing")]
		public double AveragePreswing { get; }

		[JsonPropertyName("leftPostswing")]
		public double LeftPostswing { get; }

		[JsonPropertyName("rightPostswing")]
		public double RightPostswing { get; }

		[JsonPropertyName("averagePostswing")]
		public double AveragePostswing { get; }

		[JsonPropertyName("leftTimeDependence")]
		public double LeftTimeDependence { get; }

		[JsonPropertyName("rightTimeDependence")]
		public double RightTimeDependence { get; }

		[JsonPropertyName("averageTimeDependence")]
		public double AverageTimeDependence { get; }

		[JsonPropertyName("leftAverageCut")]
		public List<double?> LeftAverageCut { get; }

		[JsonPropertyName("rightAverageCut")]
		public List<double?> RightAverageCut { get; }

		[JsonPropertyName("averageCut")]
		public List<double?> AverageCut { get; }

		[JsonPropertyName("gridAcc")]
		public List<double?> GridAcc { get; }

		[JsonPropertyName("gridCut")]
		public List<int?> GridCut { get; }

		[JsonConstructor]
		public AccuracyTracker(double accRight, double accLeft, double averageAcc, double leftSpeed, double rightSpeed, double averageSpeed, double leftHighestSpeed, double rightHighestSpeed,
			double leftPreswing, double rightPreswing, double averagePreswing, double leftPostswing, double rightPostswing, double averagePostswing, double leftTimeDependence,
			double rightTimeDependence, double averageTimeDependence, List<double?> leftAverageCut, List<double?> rightAverageCut, List<double?> averageCut, List<double?> gridAcc, List<int?> gridCut)
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
}