using System;

namespace POI.DiscordDotNet.Models.Database
{
	[Flags]
	public enum Permissions
	{
		None,
		LinkApproval,
		ForceLink
	}
}