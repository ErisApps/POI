using System.Text.Json.Serialization;

namespace POI.ThirdParty.ScoreSaber.Models.Profile;

public readonly struct BadgeDto
{
	[JsonPropertyName("image")]
	public string ImageUrl { get; }

	[JsonPropertyName("description")]
	public string Description { get; }

	[JsonConstructor]
	public BadgeDto(string imageUrl, string description)
	{
		ImageUrl = imageUrl;
		Description = description;
	}
}