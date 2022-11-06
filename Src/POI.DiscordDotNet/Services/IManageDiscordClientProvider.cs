namespace POI.DiscordDotNet.Services;

public interface IManageDiscordClientProvider : IDiscordClientProvider
{
	void Initialize();
	void Cleanup();
}