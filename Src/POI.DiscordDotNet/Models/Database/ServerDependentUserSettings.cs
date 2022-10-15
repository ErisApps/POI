using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace POI.DiscordDotNet.Models.Database
{
	public class ServerDependentUserSettings
	{
		[BsonId]
		public ObjectId Id { get; init; }

		public ulong UserId { get; init; }

		public ulong ServerId { get; init; }

		public Permissions Permissions { get; set; }

		[BsonConstructor]
		public ServerDependentUserSettings(ObjectId id, ulong userId, ulong serverId, Permissions permissions)
		{
			Id = id;
			UserId = userId;
			ServerId = serverId;
			Permissions = permissions;
		}

		public static ServerDependentUserSettings CreateDefault(ulong userId, ulong serverId)
		{
			return new ServerDependentUserSettings(ObjectId.GenerateNewId(),  userId, serverId, Permissions.None);
		}
	}
}