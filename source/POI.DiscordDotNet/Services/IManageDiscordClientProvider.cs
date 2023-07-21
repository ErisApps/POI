namespace POI.DiscordDotNet.Services;

public interface IManageDiscordClientProvider : IDiscordClientProvider
{
	Task Initialize();
	void Cleanup();
}