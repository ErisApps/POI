using System.Text.Json.Serialization;

namespace POI.Core.Models.ScoreSaber
{
	public readonly struct MetaData
	{
		[JsonPropertyName("total")]
		public uint Total { get; }

		[JsonPropertyName("page")]
		public uint Page { get; }

		[JsonPropertyName("itemsPerPage")]
		public uint ItemsPerPage { get; }

		[Newtonsoft.Json.JsonConstructor]
		public MetaData(uint total, uint page, uint itemsPerPage)
		{
			Total = total;
			Page = page;
			ItemsPerPage = itemsPerPage;
		}
	}
}