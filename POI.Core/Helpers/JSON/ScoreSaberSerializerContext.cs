using System.Text.Json.Serialization;
using POI.Core.Models.ScoreSaber.Profile;
using POI.Core.Models.ScoreSaber.Wrappers;

namespace POI.Core.Helpers.JSON
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