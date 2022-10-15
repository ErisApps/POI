using System.Collections.Generic;
using System.Text.Json.Serialization;
using POI.Core.Models.BeatSavior;

namespace POI.Core.Helpers.JSON
{
	[JsonSerializable(typeof(List<SongData>))]
	internal partial class BeatSaviorSerializerContext : JsonSerializerContext
	{

	}
}