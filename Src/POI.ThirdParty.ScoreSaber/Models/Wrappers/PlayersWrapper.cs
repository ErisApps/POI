using System.Text.Json.Serialization;
using POI.ThirdParty.ScoreSaber.Models.Profile;

namespace POI.ThirdParty.ScoreSaber.Models.Wrappers;

/// <remark>
/// This class matches the PlayerCollection object in the swagger documentation
/// </remark>
public class PlayersWrapper : BaseWrapper
{
	[JsonPropertyName("players")]
	public List<ExtendedBasicProfile> Players { get; }

	[JsonConstructor]
	public PlayersWrapper(List<ExtendedBasicProfile> players, MetaData metaData) : base(metaData)
	{
		Players = players;
	}
}