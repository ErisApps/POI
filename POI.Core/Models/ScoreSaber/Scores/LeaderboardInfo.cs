﻿using System.Text.Json.Serialization;
using NodaTime;

namespace POI.Core.Models.ScoreSaber.Scores
{
	public class LeaderboardInfo
	{
		[JsonPropertyName("id")]
		public uint Id { get; }

		[JsonPropertyName("songHash")]
		public string SongHash { get; }

		[JsonPropertyName("songName")]
		public string SongName { get; }

		[JsonPropertyName("songSubName")]
		public string SongSubName { get; }

		[JsonPropertyName("songAuthorName")]
		public string SongAuthorName { get; }

		[JsonPropertyName("levelAuthorName")]
		public string LevelAuthorName { get; }

		[JsonPropertyName("difficulty")]
		public int Difficulty { get; }

		[JsonPropertyName("difficultyRaw")]
		public string DifficultyRaw { get; }

		[JsonPropertyName("maxScore")]
		public int MaxScore { get; }

		[JsonPropertyName("createdDate")]
		public Instant CreatedDate { get; }

		[JsonPropertyName("rankedDate")]
		public Instant? RankedDate { get; }

		[JsonPropertyName("qualifiedDate")]
		public Instant? QualifiedDate { get; }

		[JsonPropertyName("lovedDate")]
		public Instant? LovedDate { get; }

		[JsonPropertyName("ranked")]
		public bool Ranked { get; }

		[JsonPropertyName("qualified")]
		public bool Qualified { get; }

		[JsonPropertyName("loved")]
		public bool Loved { get; }

		// I wanna make this an uint but for reuse, it might sometimes default to -1...
		[JsonPropertyName("maxPP")]
		public int MaxPp { get; }

		[JsonPropertyName("stars")]
		public double Stars { get; }

		[JsonPropertyName("plays")]
		public uint Plays { get; }

		[JsonPropertyName("dailyPlays")]
		public uint DailyPlays { get; }

		[JsonPropertyName("positiveModifiers")]
		public bool PositiveModifiers { get; }

		[JsonPropertyName("coverImage")]
		public string CoverImageUrl { get; }

		[JsonConstructor]
		public LeaderboardInfo(uint id, string songHash, string songName, string songSubName, string songAuthorName, string levelAuthorName, int difficulty, string difficultyRaw, int maxScore,
			Instant createdDate, Instant? rankedDate, Instant? qualifiedDate, Instant? lovedDate, bool ranked, bool qualified, bool loved, int maxPp, double stars, uint plays, uint dailyPlays,
			bool positiveModifiers, string coverImageUrl)
		{
			Id = id;
			SongHash = songHash;
			SongName = songName;
			SongSubName = songSubName;
			SongAuthorName = songAuthorName;
			LevelAuthorName = levelAuthorName;
			Difficulty = difficulty;
			DifficultyRaw = difficultyRaw;
			MaxScore = maxScore;
			CreatedDate = createdDate;
			RankedDate = rankedDate;
			QualifiedDate = qualifiedDate;
			LovedDate = lovedDate;
			Ranked = ranked;
			Qualified = qualified;
			Loved = loved;
			MaxPp = maxPp;
			Stars = stars;
			Plays = plays;
			DailyPlays = dailyPlays;
			PositiveModifiers = positiveModifiers;
			CoverImageUrl = coverImageUrl;
		}
	}
}