using MongoDB.Bson.Serialization.Attributes;

namespace POI.DiscordDotNet.Models.Database
{
	public class ServerSettings
	{
		[BsonId]
		public ulong ServerId { get; }

		public ulong? RankUpFeedChannelId { get; set; }

		public ulong? BirthdayRoleId { get; set; }

		[BsonConstructor]
		public ServerSettings(ulong serverId, ulong? rankUpFeedChannelId, ulong? birthdayRoleId)
		{
			ServerId = serverId;
			RankUpFeedChannelId = rankUpFeedChannelId;
			BirthdayRoleId = birthdayRoleId;
		}

		public static ServerSettings CreateDefault(ulong serverId)
		{
			return new ServerSettings(serverId, null, null);
		}
	}
}