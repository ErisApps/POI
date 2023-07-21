namespace POI.DiscordDotNet.Services;

public interface IAddDiscordClientFunctionality
{
	void Setup(IDiscordClientProvider discordClientProvider);
	void Cleanup(IDiscordClientProvider discordClientProvider);
}