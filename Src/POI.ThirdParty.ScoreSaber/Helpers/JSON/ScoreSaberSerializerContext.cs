using System.Text.Json.Serialization;
using POI.ThirdParty.ScoreSaber.Models.Profile;
using POI.ThirdParty.ScoreSaber.Models.Wrappers;

namespace POI.ThirdParty.ScoreSaber.Helpers.JSON;

[JsonSerializable(typeof(RefreshDto))]
[JsonSerializable(typeof(BasicProfileDto))]
[JsonSerializable(typeof(FullProfileDto))]
[JsonSerializable(typeof(LeaderboardsWrapperDto))]
[JsonSerializable(typeof(PlayerScoresWrapperDto))]
[JsonSerializable(typeof(PlayersWrapperDto))]
[JsonSerializable(typeof(ScoresWrapperDto))]
internal partial class ScoreSaberSerializerContext : JsonSerializerContext
{
}