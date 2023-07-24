using Microsoft.Extensions.DependencyInjection;

namespace POI.DiscordDotNet.Commands.SlashCommands.Profile;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddProfileSlashCommands(this IServiceCollection services) => services
		.AddTransient<BirthdaySlashCommands>();
}