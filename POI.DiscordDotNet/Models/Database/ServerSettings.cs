using MongoDB.Bson.Serialization.Attributes;

namespace POI.DiscordDotNet.Models.Database
{
	public class ServerSettings
	{
		[BsonId]
		public string ServerId { get; }

		public string? RankUpFeedChannelId { get; set; }

		public string? BirthdayRoleId { get; set; }

		[BsonConstructor]
		public ServerSettings(string serverId, string? rankUpFeedChannelId, string? birthdayRoleId)
		{
			ServerId = serverId;
			RankUpFeedChannelId = rankUpFeedChannelId;
			BirthdayRoleId = birthdayRoleId;
		}

		public static ServerSettings CreateDefault(string serverId)
		{
			return new ServerSettings(serverId, null, null);
		}
	}
}