namespace POI.Persistence.Domain
{
	public class ServerSettings
	{
		public ulong ServerId { get; init; }

		public ulong? RankUpFeedChannelId { get; set; }

		public ulong? BirthdayRoleId { get; set; }

		public static ServerSettings CreateDefault(ulong serverId)
		{
			return new ServerSettings
			{
				ServerId = serverId
			};
		}
	}
}