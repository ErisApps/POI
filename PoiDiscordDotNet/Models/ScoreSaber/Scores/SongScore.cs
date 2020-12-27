using System.Text.Json.Serialization;
using NodaTime;

namespace PoiDiscordDotNet.Models.ScoreSaber.Scores
{
	public class SongScore
	{
		[JsonPropertyName("rank")]
		public int Rank { get; init; }

		[JsonPropertyName("scoreId")]
		public int ScoreId { get; init; }

		[JsonPropertyName("score")]
		public int Score { get; init; }

		/// <remark>
		/// This might suddenly break in the future if the ScoreSaber peeps ever decide to fix this typo.
		/// </remark>
		[JsonPropertyName("unmodififiedScore")]
		public int UnmodifiedScore { get; init; }

		[JsonPropertyName("mods")]
		public string Mods { get; init; }

		[JsonPropertyName("pp")]
		public double Pp { get; init; }

		[JsonPropertyName("weight")]
		public double Weight { get; init; }

		[JsonPropertyName("timeSet")]
		public Instant TimeSet { get; init; }

		[JsonPropertyName("leaderboardId")]
		public int LeaderboardId { get; init; }

		[JsonPropertyName("songHash")]
		public string SongHash { get; init; }

		[JsonPropertyName("songName")]
		public string SongName { get; init; }

		[JsonPropertyName("songSubName")]
		public string SongSubName { get; init; }

		[JsonPropertyName("songAuthorName")]
		public string SongAuthorName { get; init; }

		[JsonPropertyName("levelAuthorName")]
		public string LevelAuthorName { get; init; }

		[JsonPropertyName("difficulty")]
		public int Difficulty { get; init; }

		[JsonPropertyName("difficultyRaw")]
		public string DifficultyRaw { get; init; }

		[JsonPropertyName("maxScore")]
		public int MaxScore { get; init; }
	}
}