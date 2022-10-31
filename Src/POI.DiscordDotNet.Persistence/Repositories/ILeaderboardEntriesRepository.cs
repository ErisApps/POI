using POI.DiscordDotNet.Persistence.Domain;

namespace POI.DiscordDotNet.Persistence.Repositories;

public interface ILeaderboardEntriesRepository
{
	Task<List<LeaderboardEntry>> GetAll(CancellationToken cts = default);
	Task DeleteAll(CancellationToken cts = default);
	Task Insert(IEnumerable<LeaderboardEntry> entries, CancellationToken cts = default);
}