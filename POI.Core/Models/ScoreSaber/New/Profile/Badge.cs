using System.Text.Json.Serialization;

namespace POI.Core.Models.ScoreSaber.New.Profile
{
	public readonly struct Badge
	{
		[JsonPropertyName("image")]
		public string ImageUrl { get; }

		[JsonPropertyName("description")]
		public string Description { get; }

		[JsonConstructor]
		public Badge(string imageUrl, string description)
		{
			ImageUrl = imageUrl;
			Description = description;
		}
	}
}