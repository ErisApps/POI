using Microsoft.Extensions.DependencyInjection;

namespace POI.DiscordDotNet.Commands.SlashCommands.ScoreSaber;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddScoreSaberSlashCommands(this IServiceCollection services) => services
		.AddTransient<ScoreSaberRecentSongCommand>()
		.AddTransient<ScoreSaberTopSongCommand>();
}