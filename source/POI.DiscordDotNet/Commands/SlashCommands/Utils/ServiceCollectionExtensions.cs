using Microsoft.Extensions.DependencyInjection;

namespace POI.DiscordDotNet.Commands.SlashCommands.Utils;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddUtilitySlashCommands(this IServiceCollection services) => services
		.AddTransient<UptimeCommand>().AddTransient<SilentMessageSlashCommands>();
}