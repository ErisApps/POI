using POI.Persistence.Domain;

namespace POI.Persistence.Repositories;

public interface IServerDependentUserSettingsRepository
{
	Task<ServerDependentUserSettings?> FindOneById(ulong userId, ulong serverId, CancellationToken cts = default);
}