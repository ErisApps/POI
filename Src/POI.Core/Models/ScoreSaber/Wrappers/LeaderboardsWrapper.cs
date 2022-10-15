using System.Collections.Generic;
using System.Text.Json.Serialization;
using POI.Core.Models.ScoreSaber.Scores;

namespace POI.Core.Models.ScoreSaber.Wrappers
{
	/// <remark>
	/// This class matches the LeaderboardInfoCollection object in the swagger documentation
	/// </remark>
	public class LeaderboardsWrapper : BaseWrapper
	{
		[JsonPropertyName("leaderboards")]
		public List<LeaderboardInfo> Leaderboards { get; }

		[JsonConstructor]
		public LeaderboardsWrapper(List<LeaderboardInfo> leaderboards, MetaData metaData) : base(metaData)
		{
			Leaderboards = leaderboards;
		}
	}
}