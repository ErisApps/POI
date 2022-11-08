namespace POI.Persistence.Domain
{
    public class ServerDependentUserSettings
    {
        public ulong DiscordUserId { get; init; }

        public ulong ServerId { get; init; }

        public Permissions Permissions { get; set; }

        public ServerDependentUserSettings(ulong discordUserId, ulong serverId, Permissions permissions = Permissions.None)
        {
	        DiscordUserId = discordUserId;
	        ServerId = serverId;
	        Permissions = permissions;
        }
    }
}