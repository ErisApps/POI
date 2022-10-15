using System.Text.Json.Serialization;

namespace POI.Core.Models.ScoreSaber.Wrappers
{
	public abstract class BaseWrapper
	{
		[JsonPropertyName("metadata")]
		public MetaData MetaData { get; }

		protected BaseWrapper(MetaData metaData)
		{
			MetaData = metaData;
		}
	}
}