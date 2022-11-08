namespace POI.Persistence.Domain
{
    public class ServerDependentUserSettings
    {
        public ulong UserId { get; init; }

        public ulong ServerId { get; init; }

        public Permissions Permissions { get; set; }

        public ServerDependentUserSettings(ulong userId, ulong serverId, Permissions permissions = Permissions.None)
        {
	        UserId = userId;
	        ServerId = serverId;
	        Permissions = permissions;
        }
    }
}