using System.Text.Json.Serialization;
using POI.Core.Models.Shared;

namespace POI.ThirdParty.ScoreSaber.Models.Scores;

public readonly struct LeaderboardDifficultyDto
{
	[JsonPropertyName("leaderboardId")]
	public int LeaderboardId { get; }

	[JsonPropertyName("gameMode")]
	public string GameMode { get; }

	[JsonPropertyName("difficulty")]
	public Difficulty Difficulty { get; }

	[JsonPropertyName("difficultyRaw")]
	public string DifficultyRaw { get; }

	[JsonConstructor]
	public LeaderboardDifficultyDto(int leaderboardId, string gameMode, Difficulty difficulty, string difficultyRaw)
	{
		LeaderboardId = leaderboardId;
		GameMode = gameMode;
		Difficulty = difficulty;
		DifficultyRaw = difficultyRaw;
	}
}