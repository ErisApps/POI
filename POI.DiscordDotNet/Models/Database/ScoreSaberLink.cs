using MongoDB.Bson.Serialization.Attributes;

namespace POI.DiscordDotNet.Models.Database
{
	// TODO: Deprecate usage as database model
	public class ScoreSaberLink
	{
		[BsonId]
		public string DiscordId { get; }

		public string ScoreSaberId { get; }

		[BsonConstructor]
		public ScoreSaberLink(string discordId, string scoreSaberId)
		{
			DiscordId = discordId;
			ScoreSaberId = scoreSaberId;
		}
	}
}