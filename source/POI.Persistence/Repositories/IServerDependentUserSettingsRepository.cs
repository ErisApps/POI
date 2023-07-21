using POI.Persistence.Domain;

namespace POI.Persistence.Repositories;

public interface IServerDependentUserSettingsRepository
{
	Task<ServerDependentUserSettings?> FindOneById(ulong discordUserId, ulong serverId, CancellationToken cts = default);
}