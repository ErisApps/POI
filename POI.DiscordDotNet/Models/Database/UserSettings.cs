using MongoDB.Bson.Serialization.Attributes;
using NodaTime;

namespace POI.DiscordDotNet.Models.Database
{
	public class UserSettings
	{
		[BsonId]
		public string DiscordId { get; set; }

		public LocalDate? Birthday { get; set; }

		public AccountLinks AccountLinks { get; set; }

		[BsonConstructor]
		public UserSettings(string discordId, LocalDate? birthday, AccountLinks accountLinks)
		{
			DiscordId = discordId;
			Birthday = birthday;
			AccountLinks = accountLinks;
		}
	}
}