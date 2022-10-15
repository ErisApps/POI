using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace POI.DiscordDotNet.Models.Database
{
	public class AccountLinks
	{
		public string? ScoreSaberId { get; set; }

		[BsonConstructor]
		[UsedImplicitly]
		public AccountLinks(string? scoreSaberId)
		{
			ScoreSaberId = scoreSaberId;
		}

		public static AccountLinks CreateDefault()
		{
			return new AccountLinks(null);
		}
	}
}