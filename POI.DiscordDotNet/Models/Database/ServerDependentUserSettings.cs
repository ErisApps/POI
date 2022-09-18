using MongoDB.Bson.Serialization.Attributes;

namespace POI.DiscordDotNet.Models.Database
{
	[BsonNoId]
	public class ServerDependentUserSettings
	{
		public ulong UserId { get; }

		public ulong ServerId { get; }

		public Permissions Permissions { get; set; }

		[BsonConstructor]
		public ServerDependentUserSettings(ulong userId, ulong serverId, Permissions permissions)
		{
			UserId = userId;
			ServerId = serverId;
			Permissions = permissions;
		}

		public static ServerDependentUserSettings CreateDefault(ulong userId, ulong serverId)
		{
			return new ServerDependentUserSettings(userId, serverId, Permissions.None);
		}
	}
}