using Microsoft.EntityFrameworkCore;
using POI.Persistence.Domain;
using POI.Persistence.EFCore.Npgsql.Infrastructure;
using POI.Persistence.Repositories;

namespace POI.Persistence.EFCore.Npgsql.Repositories;

internal class ServerDependentUserSettingsRepository : IServerDependentUserSettingsRepository
{
	private readonly IDbContextFactory<AppDbContext> _appDbContextFactory;

	public ServerDependentUserSettingsRepository(IDbContextFactory<AppDbContext> appDbContextFactory)
	{
		_appDbContextFactory = appDbContextFactory;
	}

	public async Task<ServerDependentUserSettings?> FindOneById(ulong userId, ulong serverId, CancellationToken cts)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts);
		return await context.ServerDependentUserSettings
			.AsQueryable()
			.FirstOrDefaultAsync(x => x.UserId == userId && x.ServerId == serverId, cts)
			.ConfigureAwait(false);
	}
}