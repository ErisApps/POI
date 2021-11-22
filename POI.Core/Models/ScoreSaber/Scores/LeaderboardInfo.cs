using System.Text.Json.Serialization;
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

		// TODO: Revisit this later on when the API has been changed... reflecting the actual schema, number as a date format is scuffed
		/*[JsonPropertyName("rankedDate")]
		public Instant RankedDate { get; }*/

		// TODO: Revisit this later on when the API has been changed... reflecting the actual schema, number as a date format is scuffed
		/*[JsonPropertyName("qualifiedDate")]
		public Instant QualifiedDate { get; }*/

		// TODO: Revisit this later on when the API has been changed... reflecting the actual schema, number as a date format is scuffed
		/*[JsonPropertyName("lovedDate")]
		public Instant LovedDate { get; }*/

		// TODO: Revisit this later on when the API has been changed... reflecting the actual schema, this should've been a bool
		[JsonPropertyName("ranked")]
		public uint Ranked { get; }

		// TODO: Revisit this later on when the API has been changed... reflecting the actual schema, this should've been a bool
		[JsonPropertyName("qualified")]
		public uint Qualified { get; }

		// TODO: Revisit this later on when the API has been changed... reflecting the actual schema, this should've been a bool
		[JsonPropertyName("loved")]
		public uint Loved { get; }

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
			Instant createdDate, uint ranked, uint qualified, uint loved, int maxPp, double stars, uint plays, uint dailyPlays,
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