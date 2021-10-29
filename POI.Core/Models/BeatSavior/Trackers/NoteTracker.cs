using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace POI.Core.Models.BeatSavior.Trackers
{
	public class NoteTracker
	{
		[JsonPropertyName("noteType")]
		public int NoteType { get; set; }

		[JsonPropertyName("noteDirection")]
		public int NoteDirection{ get; set; }

		[JsonPropertyName("index")]
		public int Index{ get; set; }

		[JsonPropertyName("id")]
		public int Id{ get; set; }

		[JsonPropertyName("time")]
		public float Time{ get; set; }

		[JsonPropertyName("cutType")]
		public int CutType{ get; set; }

		[JsonPropertyName("multiplier")]
		public int Multiplier{ get; set; }

		[JsonPropertyName("score")]
		public List<int> Score { get; set; } = null!;

		[JsonPropertyName("noteCenter")]
		public List<float> NoteCenter{ get; set; }= null!;

		[JsonPropertyName("noteRotation")]
		public List<float> NoteRotation{ get; set; }= null!;

		[JsonPropertyName("timeDeviation")]
		public float TimeDeviation{ get; set; }

		[JsonPropertyName("speed")]
		public float Speed{ get; set; }

		[JsonPropertyName("preswing")]
		public float Preswing{ get; set; }

		[JsonPropertyName("postswing")]
		public float Postswing{ get; set; }

		[JsonPropertyName("distanceToCenter")]
		public float DistanceToCenter{ get; set; }

		[JsonPropertyName("cutPoint")]
		public List<float> CutPoint{ get; set; }= null!;

		[JsonPropertyName("saberDir")]
		public List<float> SaberDir{ get; set; }= null!;

		[JsonPropertyName("cutNormal")]
		public List<float> CutNormal{ get; set; }= null!;

		[JsonPropertyName("timeDependence")]
		public float TimeDependence{ get; set; }
	}
}