using Microsoft.EntityFrameworkCore;
using POI.DiscordDotNet.Persistence.Domain;
using POI.DiscordDotNet.Persistence.EFCore.Npgsql.Infrastructure;
using POI.DiscordDotNet.Persistence.Repositories;

namespace POI.DiscordDotNet.Persistence.EFCore.Npgsql.Repositories;

internal class ServerSettingsRepository : IServerSettingsRepository
{
	private readonly IDbContextFactory<AppDbContext> _appDbContextFactory;

	public ServerSettingsRepository(IDbContextFactory<AppDbContext> appDbContextFactory)
	{
		_appDbContextFactory = appDbContextFactory;
	}

	public async Task<ServerSettings?> FindOneById(ulong serverId, CancellationToken cts)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts).ConfigureAwait(false);
		return await context.ServerSettings
			.AsNoTracking()
			.FirstOrDefaultAsync(x => x.ServerId == serverId, cts)
			.ConfigureAwait(false);
	}
}