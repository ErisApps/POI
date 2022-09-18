using MongoDB.Bson.Serialization.Attributes;
using NodaTime;

namespace POI.DiscordDotNet.Models.Database
{
	public class GlobalUserSettings
	{
		[BsonId]
		public string DiscordId { get; }

		public LocalDate? Birthday { get; set; }

		public AccountLinks AccountLinks { get; set; }

		[BsonConstructor]
		public GlobalUserSettings(string discordId, LocalDate? birthday = null, AccountLinks? accountLinks = null)
		{
			DiscordId = discordId;
			Birthday = birthday;
			AccountLinks = accountLinks ?? AccountLinks.CreateDefault();
		}

		public static GlobalUserSettings CreateDefault(string discordId)
		{
			return new GlobalUserSettings(discordId);
		}
	}
}