using System.Text.Json.Serialization;
using NodaTime;
using POI.Core.Models.Shared;
using POI.ThirdParty.BeatSavior.Models.Trackers;

namespace POI.ThirdParty.BeatSavior.Models
{
	public readonly struct SongData
	{
		[JsonPropertyName("_id")]
		public string Id { get; }

		[JsonPropertyName("songDataType")]
		public int SongDataType { get; }

		[JsonPropertyName("playerID")]
		public string PlayerId { get; }

		[JsonPropertyName("songID")]
		public string SongId { get; }

		[JsonPropertyName("songDifficulty")]
		public string SongDifficulty { get; }

		[JsonPropertyName("songName")]
		public string SongName { get; }

		[JsonPropertyName("songArtist")]
		public string SongArtist { get; }

		[JsonPropertyName("songMapper")]
		public string SongMapper { get; }

		[JsonPropertyName("gameMode")]
		public string GameMode { get; }

		[JsonPropertyName("songDifficultyRank")]
		public Difficulty SongDifficultyRank { get; }

		[JsonPropertyName("songSpeed")]
		public double SongSpeed { get; }

		[JsonPropertyName("songStartTime")]
		public double SongStartTime { get; }

		[JsonPropertyName("songDuration")]
		public double SongDuration { get; }

		[JsonPropertyName("trackers")]
		public SongTrackers Trackers { get; }

		[JsonPropertyName("deepTrackers")]
		public DeepTrackers? DeepTrackers { get; }

		[JsonPropertyName("timeSet")]
		public Instant TimeSet { get; }

		[JsonConstructor]
		public SongData(string id, int songDataType, string playerId, string songId, string songDifficulty, string songName, string songArtist, string songMapper, string gameMode,
			Difficulty songDifficultyRank, double songSpeed, double songStartTime, double songDuration, SongTrackers trackers, DeepTrackers? deepTrackers, Instant timeSet)
		{
			Id = id;
			SongDataType = songDataType;
			PlayerId = playerId;
			SongId = songId;
			SongDifficulty = songDifficulty;
			SongName = songName;
			SongArtist = songArtist;
			SongMapper = songMapper;
			GameMode = gameMode;
			SongDifficultyRank = songDifficultyRank;
			SongSpeed = songSpeed;
			SongStartTime = songStartTime;
			SongDuration = songDuration;
			Trackers = trackers;
			DeepTrackers = deepTrackers;
			TimeSet = timeSet;
		}
	}
}