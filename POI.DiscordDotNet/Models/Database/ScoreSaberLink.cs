using MongoDB.Bson.Serialization.Attributes;

namespace POI.DiscordDotNet.Models.Database
{
	public class ScoreSaberLink
	{
		[BsonId]
		public string DiscordId { get; set; } = null!;

		public string ScoreSaberId { get; set; } = null!;
	}
}