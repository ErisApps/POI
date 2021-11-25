using System.Collections.Generic;
using System.Text.Json.Serialization;
using NodaTime;
using POI.Core.Models.BeatSavior.Trackers;
using POI.Core.Models.Shared;

namespace POI.Core.Models.BeatSavior
{
	public class SongData
	{
		[JsonPropertyName("_id")]
		public string Id { get; init; } = string.Empty;

		[JsonPropertyName("songDataType")]
		public int SongDataType { get; init; }

		[JsonPropertyName("playerID")]
		public string PlayerId { get; init; } = string.Empty;

		[JsonPropertyName("songID")]
		public string SongId { get; init; } = string.Empty;

		[JsonPropertyName("songDifficulty")]
		public string SongDifficulty { get; init; } = string.Empty;

		[JsonPropertyName("songName")]
		public string SongName { get; init; } = string.Empty;

		[JsonPropertyName("songArtist")]
		public string SongArtist { get; init; } = string.Empty;

		[JsonPropertyName("songMapper")]
		public string SongMapper { get; init; } = string.Empty;

		[JsonPropertyName("gameMode")]
		public string GameMode { get; init; } = string.Empty;

		[JsonPropertyName("songDifficultyRank")]
		public Difficulty SongDifficultyRank { get; init; }

		[JsonPropertyName("songSpeed")]
		public double SongSpeed { get; init; }

		[JsonPropertyName("songStartTime")]
		public double SongStartTime { get; init; }

		[JsonPropertyName("songDuration")]
		public double SongDuration { get; init; }

		[JsonPropertyName("trackers")]
		public SongTrackers Trackers { get; init; } = null!;

		[JsonPropertyName("deepTrackers")]
		public DeepTrackers? DeepTrackers { get; init; }

		[JsonPropertyName("timeSet")]
		public Instant TimeSet { get; init; }
	}
}