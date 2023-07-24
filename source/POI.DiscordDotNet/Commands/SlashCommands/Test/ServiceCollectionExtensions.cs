using Microsoft.Extensions.DependencyInjection;

namespace POI.DiscordDotNet.Commands.SlashCommands.Test;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddTestSlashCommands(this IServiceCollection services) => services
		.AddTransient<PacmanCommand>();
}