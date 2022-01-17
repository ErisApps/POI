using System.Collections.Generic;
using System.Text.Json.Serialization;
using POI.Core.Models.ScoreSaber.Scores;

namespace POI.Core.Models.ScoreSaber.Wrappers
{
	public class PlayerScoresWrapper : BaseWrapper
	{
		[JsonPropertyName("playerScores")]
		public List<PlayerScore> PlayerScores { get; }

		[JsonConstructor]
		public PlayerScoresWrapper(List<PlayerScore> playerScores, MetaData metaData) : base(metaData)
		{
			PlayerScores = playerScores;
		}
	}
}