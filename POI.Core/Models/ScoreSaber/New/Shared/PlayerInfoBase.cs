using System.Text.Json.Serialization;

namespace POI.Core.Models.ScoreSaber.New.Shared
{
	public class PlayerInfoBase
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
		public PlayerInfoBase(string id, string name, string profilePicture, string country)
		{
			Id = id;
			Name = name;
			ProfilePicture = profilePicture;
			Country = country;
		}
	}
}