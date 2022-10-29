using System.Text.Json.Serialization;
using POI.ThirdParty.ScoreSaber.Models.Scores;

namespace POI.ThirdParty.ScoreSaber.Models.Wrappers;

/// <remark>
/// This class matches the PlayerScoreCollection object in the swagger documentation
/// </remark>
public class PlayerScoresWrapper : BaseWrapper
{
	[JsonPropertyName("playerScores")]
	public List<PlayerScore> PlayerScores { get; }

	[JsonConstructor]
	public PlayerScoresWrapper(List<PlayerScore> playerScores, MetaData metaData) : base(metaData)
	{
		PlayerScores = playerScores;
	}
}