using POI.DiscordDotNet.Persistence.Domain;

namespace POI.DiscordDotNet.Persistence.Repositories;

public interface IServerDependentUserSettingsRepository
{
	Task<ServerDependentUserSettings?> FindOneById(ulong userId, ulong serverId, CancellationToken cts = default);
}