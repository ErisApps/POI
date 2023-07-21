using System.Text.Json.Serialization;
using POI.ThirdParty.ScoreSaber.Models.Scores;

namespace POI.ThirdParty.ScoreSaber.Models.Wrappers;

/// <remark>
/// This class matches the PlayerScoreCollection object in the swagger documentation
/// </remark>
public class PlayerScoresWrapperDto : BaseWrapperDto
{
	[JsonPropertyName("playerScores")]
	public List<PlayerScoreDto> PlayerScores { get; }

	[JsonConstructor]
	public PlayerScoresWrapperDto(List<PlayerScoreDto> playerScores, MetaDataDto metaData) : base(metaData)
	{
		PlayerScores = playerScores;
	}
}