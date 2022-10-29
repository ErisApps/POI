using System.Text.Json.Serialization;
using POI.ThirdParty.ScoreSaber.Models.Scores;

namespace POI.ThirdParty.ScoreSaber.Models.Wrappers;

/// <remark>
/// This class matches the ScoreCollection object in the swagger documentation
/// </remark>
public class ScoresWrapper : BaseWrapper
{
	[JsonPropertyName("scores")]
	public List<Score> Scores { get; }

	[JsonConstructor]
	public ScoresWrapper(List<Score> scores, MetaData metaData) : base(metaData)
	{
		Scores = scores;
	}
}