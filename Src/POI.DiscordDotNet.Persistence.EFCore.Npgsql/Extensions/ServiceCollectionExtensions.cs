using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using POI.DiscordDotNet.Persistence.EFCore.Npgsql.Infrastructure;
using POI.DiscordDotNet.Persistence.EFCore.Npgsql.Repositories;
using POI.DiscordDotNet.Persistence.Repositories;

namespace POI.DiscordDotNet.Persistence.EFCore.Npgsql.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
	{
		if (string.IsNullOrWhiteSpace(connectionString))
		{
			throw new InvalidDataException("Connection string cannot be null or empty.");
		}

		services.AddDbContextFactory<AppDbContext>(options => options.UseNpgsql(
			connectionString,
			o => o.UseNodaTime()));

		services.TryAddScoped<IGlobalUserSettingsRepository, GlobalUserSettingsRepository>();
		services.TryAddScoped<ILeaderboardEntriesRepository, LeaderboardEntriesRepository>();
		services.TryAddScoped<IServerDependentUserSettingsRepository, ServerDependentUserSettingsRepository>();
		services.TryAddScoped<IServerSettingsRepository, ServerSettingsRepository>();

		return services;
	}
}