using POI.DiscordDotNet.Persistence.Domain;

namespace POI.DiscordDotNet.Persistence.Repositories;

public interface IServerSettingsRepository
{
	Task<ServerSettings?> FindOneById(ulong serverId, CancellationToken cts = default);
}