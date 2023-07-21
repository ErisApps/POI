using System.Text.Json.Serialization;

namespace POI.ThirdParty.ScoreSaber.Models.Shared;

public class PlayerInfoBaseDto
{
	[JsonPropertyName("id")]
	public string Id { get; }

	[JsonPropertyName("name")]
	public string Name { get; }

	[JsonPropertyName("profilePicture")]
	public string ProfilePicture { get; }

	[JsonPropertyName("country")]
	public string Country { get; }

	[JsonConstructor]
	public PlayerInfoBaseDto(string id, string name, string profilePicture, string country)
	{
		Id = id;
		Name = name;
		ProfilePicture = profilePicture;
		Country = country;
	}
}