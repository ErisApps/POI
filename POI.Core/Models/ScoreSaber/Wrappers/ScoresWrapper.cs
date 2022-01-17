using System.Collections.Generic;
using System.Text.Json.Serialization;
using POI.Core.Models.ScoreSaber.Scores;

namespace POI.Core.Models.ScoreSaber.Wrappers
{
	public class ScoresWrapper : BaseWrapper
	{
		[JsonPropertyName("scores")]
		public List<Score> Scores { get; }

		[JsonConstructor]
		public ScoresWrapper(List<Score> scores, MetaData metaData) : base(metaData)
		{
			Scores = scores;
		}
	}
}