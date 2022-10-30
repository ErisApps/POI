using System.ComponentModel.DataAnnotations;

namespace POI.DiscordDotNet.Persistence.Domain
{
	public class ServerSettings
	{
		[Key]
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