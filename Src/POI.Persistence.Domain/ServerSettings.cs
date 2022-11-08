namespace POI.Persistence.Domain
{
	public class ServerSettings
	{
		public ulong ServerId { get; init; }

		public ulong? RankUpFeedChannelId { get; set; }

		public ulong? BirthdayRoleId { get; set; }

		public ServerSettings(ulong serverId, ulong? rankUpFeedChannelId = null, ulong? birthdayRoleId = null)
		{
			ServerId = serverId;
			RankUpFeedChannelId = rankUpFeedChannelId;
			BirthdayRoleId = birthdayRoleId;
		}
	}
}