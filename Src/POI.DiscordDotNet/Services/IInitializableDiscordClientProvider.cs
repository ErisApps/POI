namespace POI.DiscordDotNet.Services;

public interface IInitializableDiscordClientProvider : IDiscordClientProvider
{
	Task Initialize();
}