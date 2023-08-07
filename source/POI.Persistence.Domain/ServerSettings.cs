namespace POI.Persistence.Domain
{
	public class ServerSettings
	{
		public ulong ServerId { get; init; }

		public ulong? RankUpFeedChannelId { get; set; }

		public ulong? BirthdayRoleId { get; set; }

		public ulong? StarboardChannelId { get; set; }

		public ulong? EventsChannelId { get; set; }

		public uint? StarboardEmojiCount { get; set; }

		public ServerSettings(ulong serverId, ulong? rankUpFeedChannelId = null, ulong? birthdayRoleId = null, ulong? starboardChannelId = null, ulong? eventsChannelId = null, uint? starboardEmojiCount = null)
		{
			ServerId = serverId;
			RankUpFeedChannelId = rankUpFeedChannelId;
			BirthdayRoleId = birthdayRoleId;
			StarboardChannelId = starboardChannelId;
			EventsChannelId = eventsChannelId;
			StarboardEmojiCount = starboardEmojiCount;
		}
	}
}