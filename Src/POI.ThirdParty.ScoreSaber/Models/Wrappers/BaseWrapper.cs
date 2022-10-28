using System.Text.Json.Serialization;

namespace POI.ThirdParty.ScoreSaber.Models.Wrappers
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