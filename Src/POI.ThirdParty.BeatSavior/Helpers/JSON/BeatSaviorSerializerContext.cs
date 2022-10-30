using System.Text.Json.Serialization;
using POI.ThirdParty.BeatSavior.Models;

namespace POI.ThirdParty.BeatSavior.Helpers.JSON;

[JsonSerializable(typeof(List<SongDataDto>))]
internal partial class BeatSaviorSerializerContext : JsonSerializerContext
{
}