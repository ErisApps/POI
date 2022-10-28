using System.Text.Json.Serialization;
using POI.ThirdParty.ScoreSaber.Models.Profile;
using POI.ThirdParty.ScoreSaber.Models.Wrappers;

namespace POI.ThirdParty.ScoreSaber.Helpers.JSON
{
	[JsonSerializable(typeof(Refresh))]
	[JsonSerializable(typeof(BasicProfile))]
	[JsonSerializable(typeof(FullProfile))]
	[JsonSerializable(typeof(LeaderboardsWrapper))]
	[JsonSerializable(typeof(PlayerScoresWrapper))]
	[JsonSerializable(typeof(PlayersWrapper))]
	[JsonSerializable(typeof(ScoresWrapper))]
	internal partial class ScoreSaberSerializerContext : JsonSerializerContext
	{
	}
}