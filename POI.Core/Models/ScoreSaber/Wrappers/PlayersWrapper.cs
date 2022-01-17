using System.Collections.Generic;
using System.Text.Json.Serialization;
using POI.Core.Models.ScoreSaber.Profile;

namespace POI.Core.Models.ScoreSaber.Wrappers
{
	/// <remark>
	/// This class matches the PlayersCollection object in the swagger documentation
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
}