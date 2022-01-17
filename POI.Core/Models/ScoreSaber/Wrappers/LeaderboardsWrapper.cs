using System.Collections.Generic;
using System.Text.Json.Serialization;
using POI.Core.Models.ScoreSaber.Scores;

namespace POI.Core.Models.ScoreSaber.Wrappers
{
	public class LeaderboardsWrapper : BaseWrapper
	{
		[JsonPropertyName("leaderboards")]
		public List<LeaderboardInfo> Leaderboards { get; }

		public LeaderboardsWrapper(List<LeaderboardInfo> leaderboards, MetaData metaData) : base(metaData)
		{
			Leaderboards = leaderboards;
		}
	}
}