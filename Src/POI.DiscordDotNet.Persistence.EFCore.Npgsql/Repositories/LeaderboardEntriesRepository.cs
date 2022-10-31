using Microsoft.EntityFrameworkCore;
using POI.DiscordDotNet.Persistence.Domain;
using POI.DiscordDotNet.Persistence.EFCore.Npgsql.Infrastructure;
using POI.DiscordDotNet.Persistence.Repositories;

namespace POI.DiscordDotNet.Persistence.EFCore.Npgsql.Repositories;

internal class LeaderboardEntriesRepository : ILeaderboardEntriesRepository
{
	private readonly IDbContextFactory<AppDbContext> _appDbContextFactory;

	public LeaderboardEntriesRepository(IDbContextFactory<AppDbContext> appDbContextFactory)
	{
		_appDbContextFactory = appDbContextFactory;
	}


	public async Task<List<LeaderboardEntry>> GetAll(CancellationToken cts = default)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts).ConfigureAwait(false);
		return await context.LeaderboardEntries
			.AsNoTracking()
			.ToListAsync(cts)
			.ConfigureAwait(false);
	}

	public async Task DeleteAll(CancellationToken cts = default)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts).ConfigureAwait(false);
		await context.Database.ExecuteSqlAsync($"DELETE FROM LeaderboardEntries", cts).ConfigureAwait(false);
	}

	public async Task Insert(IEnumerable<LeaderboardEntry> entries, CancellationToken cts = default)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts).ConfigureAwait(false);
		await context.AddRangeAsync(entries, cts).ConfigureAwait(false);
	}
}