using System.Text.Json.Serialization;
using POI.ThirdParty.BeatSavior.Models;

namespace POI.ThirdParty.BeatSavior.Helpers.JSON;

[JsonSerializable(typeof(List<SongData>))]
internal partial class BeatSaviorSerializerContext : JsonSerializerContext
{
}