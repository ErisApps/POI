using Microsoft.EntityFrameworkCore;
using POI.Persistence.Domain;
using POI.Persistence.EFCore.Npgsql.Infrastructure;
using POI.Persistence.Repositories;

namespace POI.Persistence.EFCore.Npgsql.Repositories;

internal class StarboardMessagesRepository : IStarboardMessagesRepository
{
	private readonly IDbContextFactory<AppDbContext> _appDbContextFactory;

	public StarboardMessagesRepository(IDbContextFactory<AppDbContext> appDbContextFactory)
	{
		_appDbContextFactory = appDbContextFactory;
	}

	public async Task<StarboardMessages?> FindOneByServerIdAndChannelIdAndMessageId(ulong serverId, ulong channelId, ulong messageId, CancellationToken cts = default)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts).ConfigureAwait(false);
		return await context.StarboardMessages
			.AsNoTracking()
			.FirstOrDefaultAsync(x => x.ServerId == serverId && x.ChannelId == channelId && x.MessageId == messageId, cts)
			.ConfigureAwait(false);
	}

	public async Task Insert(StarboardMessages entry, CancellationToken cts = default)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts).ConfigureAwait(false);
		await context.AddAsync(entry, cts).ConfigureAwait(false);
		await context.SaveChangesAsync(cts).ConfigureAwait(false);
	}
}