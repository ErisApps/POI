using System.Text.Json.Serialization;

namespace POI.ThirdParty.ScoreSaber.Models.Scores;

public readonly struct PlayerScoreDto
{
	[JsonPropertyName("score")]
	public ScoreDto Score { get; }

	[JsonPropertyName("leaderboard")]
	public LeaderboardInfoDto Leaderboard { get; }

	[JsonConstructor]
	public PlayerScoreDto(ScoreDto score, LeaderboardInfoDto leaderboard)
	{
		Score = score;
		Leaderboard = leaderboard;
	}
}