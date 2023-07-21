using System.Text.Json.Serialization;

namespace POI.ThirdParty.ScoreSaber.Models;

public readonly struct MetaDataDto
{
	[JsonPropertyName("total")]
	public uint Total { get; }

	[JsonPropertyName("page")]
	public uint Page { get; }

	[JsonPropertyName("itemsPerPage")]
	public uint ItemsPerPage { get; }

	[JsonConstructor]
	public MetaDataDto(uint total, uint page, uint itemsPerPage)
	{
		Total = total;
		Page = page;
		ItemsPerPage = itemsPerPage;
	}
}