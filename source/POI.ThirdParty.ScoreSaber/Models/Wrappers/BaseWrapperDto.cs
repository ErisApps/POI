using System.Text.Json.Serialization;

namespace POI.ThirdParty.ScoreSaber.Models.Wrappers;

public abstract class BaseWrapperDto
{
	[JsonPropertyName("metadata")]
	public MetaDataDto MetaData { get; }

	protected BaseWrapperDto(MetaDataDto metaData)
	{
		MetaData = metaData;
	}
}