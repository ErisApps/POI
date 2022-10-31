namespace POI.DiscordDotNet.Persistence.Domain
{
    public class ServerDependentUserSettings
    {
        public ulong UserId { get; init; }

        public ulong ServerId { get; init; }

        public Permissions Permissions { get; set; } = Permissions.None;

        public static ServerDependentUserSettings CreateDefault(ulong userId, ulong serverId)
        {
            return new ServerDependentUserSettings
            {
                UserId = userId,
                ServerId = serverId
            };
        }
    }
}