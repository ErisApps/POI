using System.Text.Json.Serialization;
using POI.ThirdParty.ScoreSaber.Models.Scores;

namespace POI.ThirdParty.ScoreSaber.Models.Wrappers
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