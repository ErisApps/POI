using Microsoft.EntityFrameworkCore;
using POI.Persistence.Domain;
using POI.Persistence.EFCore.Npgsql.Infrastructure;
using POI.Persistence.Repositories;

namespace POI.Persistence.EFCore.Npgsql.Repositories;

internal class LinkRequestTokensRepository: ILinkRequestTokensRepository
{
	private readonly IDbContextFactory<AppDbContext> _appDbContextFactory;

	public LinkRequestTokensRepository(IDbContextFactory<AppDbContext> appDbContextFactory)
	{
		_appDbContextFactory = appDbContextFactory;
	}

	public async Task<ulong> GetDiscordIdByToken(string token)
	{
		await DeleteOldTokens().ConfigureAwait(false);
		await using var context = await _appDbContextFactory.CreateDbContextAsync().ConfigureAwait(false);
		var linkRequestToken = await context.LinkRequestTokens
			.AsNoTracking()
			.AsQueryable()
			.FirstOrDefaultAsync(x => x.LoginToken == token)
			.ConfigureAwait(false);

		if (linkRequestToken == null)
		{
			throw new ArgumentException("Token not found");
		}

		var discordId = linkRequestToken.DiscordId;

		await DeleteTokensByDiscordId(discordId).ConfigureAwait(false);
		return discordId;
	}

	public async Task<string> CreateToken(ulong discordUserId)
	{
		await DeleteOldTokens().ConfigureAwait(false);
		await using var context = await _appDbContextFactory.CreateDbContextAsync().ConfigureAwait(false);
		var token = Guid.NewGuid().ToString();
		var linkRequestToken = new LinkRequestToken{LoginToken = token, DiscordId = discordUserId, CreatedAt = DateTimeOffset.UtcNow};
		await context.LinkRequestTokens.AddAsync(linkRequestToken).ConfigureAwait(false);
		await context.SaveChangesAsync().ConfigureAwait(false);
		return token;
	}

	private async Task DeleteTokensByDiscordId(ulong discordId, CancellationToken cts = default)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts).ConfigureAwait(false);
		var tokens = await context.LinkRequestTokens
			.Where(x => x.DiscordId == discordId)
			.ToListAsync(cts)
			.ConfigureAwait(false);

		context.LinkRequestTokens.RemoveRange(tokens);
		await context.SaveChangesAsync(cts).ConfigureAwait(false);
	}

	private async Task DeleteOldTokens(CancellationToken cts = default)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts).ConfigureAwait(false);
		var tokens = await context.LinkRequestTokens
			.Where(l => l.CreatedAt < DateTimeOffset.UtcNow.AddMinutes(-15))
			.ToListAsync(cts)
			.ConfigureAwait(false);

		context.LinkRequestTokens.RemoveRange(tokens);
		await context.SaveChangesAsync(cts).ConfigureAwait(false);
	}
}