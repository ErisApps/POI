namespace POI.DiscordDotNet.Services
{
    public interface IAddDiscordClientFunctionality
    {
        Task Setup(IDiscordClientProvider discordClientProvider);
    }
}