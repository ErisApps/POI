using System.Text.Json.Serialization;
using POI.Core.Models.ScoreSaber.Shared;

namespace POI.Core.Models.ScoreSaber.Scores
{
	public class LeaderboardPlayer : PlayerInfoBase
	{
		[JsonPropertyName("role")]
		public string Role { get; }

		[JsonPropertyName("permissions")]
		public uint Permissions { get; }

		[JsonConstructor]
		public LeaderboardPlayer(string id, string name, string profilePicture, string country, string role, uint permissions)
			: base(id, name, profilePicture, country)
		{
			Role = role;
			Permissions = permissions;
		}
	}
}