using System.Collections.Generic;
using System.Text.Json.Serialization;
using POI.Core.Models.ScoreSaber.Profile;
using POI.Core.Models.ScoreSaber.Scores;

namespace POI.Core.Helpers.JSON
{
	[JsonSerializable(typeof(Refresh))]
	[JsonSerializable(typeof(List<BasicProfile>))]
	[JsonSerializable(typeof(FullProfile))]
	[JsonSerializable(typeof(List<PlayerScore>))]
	internal partial class ScoreSaberSerializerContext : JsonSerializerContext
	{
	}
}