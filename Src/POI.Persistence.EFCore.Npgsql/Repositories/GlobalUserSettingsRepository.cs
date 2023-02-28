using Microsoft.EntityFrameworkCore;
using NodaTime;
using POI.Persistence.Domain;
using POI.Persistence.EFCore.Npgsql.Infrastructure;
using POI.Persistence.Models.AccountLink;
using POI.Persistence.Repositories;

namespace POI.Persistence.EFCore.Npgsql.Repositories;

internal class GlobalUserSettingsRepository : IGlobalUserSettingsRepository
{
	private readonly IDbContextFactory<AppDbContext> _appDbContextFactory;

	public GlobalUserSettingsRepository(IDbContextFactory<AppDbContext> appDbContextFactory)
	{
		_appDbContextFactory = appDbContextFactory;
	}

	public async Task<GlobalUserSettings?> LookupSettingsByDiscordId(ulong discordUserId, CancellationToken cts)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts).ConfigureAwait(false);
		return await context.GlobalUserSettings
			.AsNoTracking()
			.AsQueryable()
			.FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId, cts)
			.ConfigureAwait(false);
	}

	public async Task<GlobalUserSettings?> LookupSettingsByScoreSaberId(string scoreSaberId, CancellationToken cts)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts).ConfigureAwait(false);
		return await context.GlobalUserSettings
			.AsNoTracking()
			.AsQueryable()
			.FirstOrDefaultAsync(x => x.ScoreSaberId == scoreSaberId, cts)
			.ConfigureAwait(false);
	}

	public async Task<List<ScoreSaberAccountLink>> GetAllScoreSaberAccountLinks(CancellationToken cts)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts).ConfigureAwait(false);
		return await context.GlobalUserSettings
			.AsNoTracking()
			.AsQueryable()
			.Where(x => x.ScoreSaberId != null)
			.Select(x => new ScoreSaberAccountLink(x.DiscordUserId, x.ScoreSaberId!))
			.ToListAsync(cts)
			.ConfigureAwait(false);
	}

	public async Task CreateOrUpdateScoreSaberLink(ulong discordUserId, string scoreSaberId, CancellationToken cts = default)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts).ConfigureAwait(false);
		var globalUserSettings = await context.GlobalUserSettings
			.FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId, cts)
			.ConfigureAwait(false);

		if (globalUserSettings == null)
		{
			globalUserSettings = new GlobalUserSettings(discordUserId, scoreSaberId: scoreSaberId);
			await context.GlobalUserSettings.AddAsync(globalUserSettings, cts).ConfigureAwait(false);
		}
		else
		{
			globalUserSettings.ScoreSaberId = scoreSaberId;
		}

		await context.SaveChangesAsync(cts).ConfigureAwait(false);
	}

	public async Task<List<GlobalUserSettings>> GetAllBirthdayGirls(int dayOfMonth, int month, CancellationToken cts)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts).ConfigureAwait(false);
		return await context.GlobalUserSettings
			.AsQueryable()
			.Where(x => x.Birthday.HasValue && x.Birthday.Value.Month == month && x.Birthday.Value.Day == dayOfMonth)
			.ToListAsync(cts)
			.ConfigureAwait(false);
	}

	public async Task UpdateBirthday(ulong discordUserId, LocalDate? birthday, CancellationToken cts)
	{
		await using var context = await _appDbContextFactory.CreateDbContextAsync(cts).ConfigureAwait(false);
		var globalUserSettings = await context.GlobalUserSettings
			.FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId, cts)
			.ConfigureAwait(false);

		if (globalUserSettings == null)
		{
			globalUserSettings = new GlobalUserSettings(discordUserId, birthday: birthday);
			await context.GlobalUserSettings.AddAsync(globalUserSettings, cts).ConfigureAwait(false);
		}
		else
		{
			globalUserSettings.Birthday = birthday;
		}

		await context.SaveChangesAsync(cts).ConfigureAwait(false);
	}
}