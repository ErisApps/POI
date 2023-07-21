using System.Text.Json.Serialization;
using POI.ThirdParty.ScoreSaber.Models.Scores;

namespace POI.ThirdParty.ScoreSaber.Models.Wrappers;

/// <remark>
/// This class matches the LeaderboardInfoCollection object in the swagger documentation
/// </remark>
public class LeaderboardsWrapperDto : BaseWrapperDto
{
	[JsonPropertyName("leaderboards")]
	public List<LeaderboardInfoDto> Leaderboards { get; }

	[JsonConstructor]
	public LeaderboardsWrapperDto(List<LeaderboardInfoDto> leaderboards, MetaDataDto metaData) : base(metaData)
	{
		Leaderboards = leaderboards;
	}
}