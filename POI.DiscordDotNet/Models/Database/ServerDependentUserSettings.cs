using MongoDB.Bson.Serialization.Attributes;

namespace POI.DiscordDotNet.Models.Database
{
	[BsonNoId]
	public class ServerDependentUserSettings
	{
		public string UserId { get; }

		public string ServerId { get; }

		public Permissions Permissions { get; set; } = Permissions.None;

		[BsonConstructor]
		public ServerDependentUserSettings(string userId, string serverId, Permissions permissions)
		{
			UserId = userId;
			ServerId = serverId;
			Permissions = permissions;
		}

		public static ServerDependentUserSettings CreateDefault(string userId, string serverId)
		{
			return new ServerDependentUserSettings(userId, serverId, Permissions.None);
		}
	}
}