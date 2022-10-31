using Microsoft.EntityFrameworkCore;
using NodaTime;
using POI.DiscordDotNet.Persistence.Domain;
using POI.DiscordDotNet.Persistence.EFCore.Npgsql.Infrastructure;
using POI.DiscordDotNet.Persistence.Repositories;

namespace POI.DiscordDotNet.Persistence.EFCore.Npgsql.Repositories;

internal class GlobalUserSettingsRepository : IGlobalUserSettingsRepository
{
	private readonly IDbContextFactory<AppDbContext> _appDbContextFactory;

	public GlobalUserSettingsRepository(IDbContextFactory<AppDbContext> appDbContextFactory)
	{
		_appDbContextFactory = appDbContextFactory;
	}

	public async Task<GlobalUserSettings?> LookupSettingsByDiscordId(ulong discordId, CancellationToken cts)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts).ConfigureAwait(false);
		return await context.GlobalUserSettings
			.AsNoTracking()
			.Include(x => x.AccountLinks)
			.AsNoTracking()
			.AsQueryable()
			.FirstOrDefaultAsync(x => x.UserId == discordId, cts)
			.ConfigureAwait(false);
	}

	public async Task<GlobalUserSettings?> LookupSettingsByScoreSaberId(string scoreSaberId, CancellationToken cts)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts).ConfigureAwait(false);
		return await context.GlobalUserSettings
			.AsNoTracking()
			.Include(x => x.AccountLinks)
			.AsNoTracking()
			.AsQueryable()
			.FirstOrDefaultAsync(x => x.AccountLinks.ScoreSaberId == scoreSaberId, cts)
			.ConfigureAwait(false);
	}

	public async Task<List<GlobalUserSettings>> GetAllBirthdayGirls(LocalDate birthdayDate, CancellationToken cts)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts).ConfigureAwait(false);
		return await context.GlobalUserSettings
			.AsQueryable()
			.Where(x => x.Birthday.HasValue && x.Birthday.Value.Month == birthdayDate.Month && x.Birthday.Value.Day == birthdayDate.Day)
			.ToListAsync(cts)
			.ConfigureAwait(false);
	}

	public async Task UpdateBirthday(ulong discordId, LocalDate? birthday, CancellationToken cts)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts).ConfigureAwait(false);
		var globalUserSettings = await context.GlobalUserSettings
			.FirstOrDefaultAsync(x => x.UserId == discordId, cts)
			.ConfigureAwait(false);


		if (globalUserSettings == null)
		{
			globalUserSettings = GlobalUserSettings.CreateDefault(discordId);
			await context.GlobalUserSettings.AddAsync(globalUserSettings, cts).ConfigureAwait(false);
		}

		globalUserSettings.Birthday = birthday;

		await context.SaveChangesAsync(cts).ConfigureAwait(false);
	}
}