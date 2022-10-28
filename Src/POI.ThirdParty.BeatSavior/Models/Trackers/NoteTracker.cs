using System.Text.Json.Serialization;

namespace POI.ThirdParty.BeatSavior.Models.Trackers
{
	public readonly struct NoteTracker
	{
		[JsonPropertyName("noteType")]
		public int NoteType { get; }

		[JsonPropertyName("noteDirection")]
		public int NoteDirection { get; }

		[JsonPropertyName("index")]
		public int Index { get; }

		[JsonPropertyName("id")]
		public int Id { get; }

		[JsonPropertyName("time")]
		public float Time { get; }

		[JsonPropertyName("cutType")]
		public int CutType { get; }

		[JsonPropertyName("multiplier")]
		public int Multiplier { get; }

		[JsonPropertyName("score")]
		public List<int> Score { get; }

		[JsonPropertyName("noteCenter")]
		public List<float> NoteCenter { get; }

		[JsonPropertyName("noteRotation")]
		public List<float> NoteRotation { get; }

		[JsonPropertyName("timeDeviation")]
		public float TimeDeviation { get; }

		[JsonPropertyName("speed")]
		public float Speed { get; }

		[JsonPropertyName("preswing")]
		public float Preswing { get; }

		[JsonPropertyName("postswing")]
		public float Postswing { get; }

		[JsonPropertyName("distanceToCenter")]
		public float DistanceToCenter { get; }

		[JsonPropertyName("cutPoint")]
		public List<float> CutPoint { get; }

		[JsonPropertyName("saberDir")]
		public List<float> SaberDir { get; }

		[JsonPropertyName("cutNormal")]
		public List<float> CutNormal { get; }

		[JsonPropertyName("timeDependence")]
		public float TimeDependence { get; }

		[JsonConstructor]
		public NoteTracker(int noteType, int noteDirection, int index, int id, float time, int cutType, int multiplier, List<int> score, List<float> noteCenter, List<float> noteRotation,
			float timeDeviation, float speed, float preswing, float postswing, float distanceToCenter, List<float> cutPoint, List<float> saberDir, List<float> cutNormal, float timeDependence)
		{
			NoteType = noteType;
			NoteDirection = noteDirection;
			Index = index;
			Id = id;
			Time = time;
			CutType = cutType;
			Multiplier = multiplier;
			Score = score;
			NoteCenter = noteCenter;
			NoteRotation = noteRotation;
			TimeDeviation = timeDeviation;
			Speed = speed;
			Preswing = preswing;
			Postswing = postswing;
			DistanceToCenter = distanceToCenter;
			CutPoint = cutPoint;
			SaberDir = saberDir;
			CutNormal = cutNormal;
			TimeDependence = timeDependence;
		}
	}
}