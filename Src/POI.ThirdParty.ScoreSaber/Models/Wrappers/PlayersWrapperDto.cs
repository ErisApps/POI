using System.Text.Json.Serialization;
using POI.ThirdParty.ScoreSaber.Models.Profile;

namespace POI.ThirdParty.ScoreSaber.Models.Wrappers;

/// <remark>
/// This class matches the PlayerCollection object in the swagger documentation
/// </remark>
public class PlayersWrapperDto : BaseWrapperDto
{
	[JsonPropertyName("players")]
	public List<ExtendedBasicProfileDto> Players { get; }

	[JsonConstructor]
	public PlayersWrapperDto(List<ExtendedBasicProfileDto> players, MetaDataDto metaData) : base(metaData)
	{
		Players = players;
	}
}