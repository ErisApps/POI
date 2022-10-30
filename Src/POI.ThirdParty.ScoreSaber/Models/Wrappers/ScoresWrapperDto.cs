using System.Text.Json.Serialization;
using POI.ThirdParty.ScoreSaber.Models.Scores;

namespace POI.ThirdParty.ScoreSaber.Models.Wrappers;

/// <remark>
/// This class matches the ScoreCollection object in the swagger documentation
/// </remark>
public class ScoresWrapperDto : BaseWrapperDto
{
	[JsonPropertyName("scores")]
	public List<ScoreDto> Scores { get; }

	[JsonConstructor]
	public ScoresWrapperDto(List<ScoreDto> scores, MetaDataDto metaData) : base(metaData)
	{
		Scores = scores;
	}
}