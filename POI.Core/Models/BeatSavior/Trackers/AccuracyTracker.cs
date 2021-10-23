using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace POI.Core.Models.BeatSavior.Trackers
{
	public class AccuracyTracker
	{
		[JsonPropertyName("accRight")]
		public double AccRight { get; init; }

		[JsonPropertyName("accLeft")]
		public double AccLeft { get; init; }

		[JsonPropertyName("averageAcc")]
		public double AverageAcc { get; init; }

		[JsonPropertyName("leftSpeed")]
		public double LeftSpeed { get; init; }

		[JsonPropertyName("rightSpeed")]
		public double RightSpeed { get; init; }

		[JsonPropertyName("averageSpeed")]
		public double AverageSpeed { get; init; }

		[JsonPropertyName("leftHighestSpeed")]
		public double LeftHighestSpeed { get; init; }

		[JsonPropertyName("rightHighestSpeed")]
		public double RightHighestSpeed { get; init; }

		[JsonPropertyName("leftPreswing")]
		public double LeftPreswing { get; init; }

		[JsonPropertyName("rightPreswing")]
		public double RightPreswing { get; init; }

		[JsonPropertyName("averagePreswing")]
		public double AveragePreswing { get; init; }

		[JsonPropertyName("leftPostswing")]
		public double LeftPostswing { get; init; }

		[JsonPropertyName("rightPostswing")]
		public double RightPostswing { get; init; }

		[JsonPropertyName("averagePostswing")]
		public double AveragePostswing { get; init; }

		[JsonPropertyName("leftTimeDependence")]
		public double LeftTimeDependence { get; init; }

		[JsonPropertyName("rightTimeDependence")]
		public double RightTimeDependence { get; init; }

		[JsonPropertyName("averageTimeDependence")]
		public double AverageTimeDependence { get; init; }

		[JsonPropertyName("leftAverageCut")]
		public List<double?> LeftAverageCut { get; init; } = null!;

		[JsonPropertyName("rightAverageCut")]
		public List<double?> RightAverageCut { get; init; } = null!;

		[JsonPropertyName("averageCut")]
		public List<double?> AverageCut { get; init; } = null!;

		[JsonPropertyName("gridAcc")]
		public List<double?> GridAcc { get; init; } = null!;

		[JsonPropertyName("gridCut")]
		public List<int?> GridCut { get; init; } = null!;
	}
}