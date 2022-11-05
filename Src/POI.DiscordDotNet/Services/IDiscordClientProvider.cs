using DSharpPlus;

namespace POI.DiscordDotNet.Services
{
    public interface IDiscordClientProvider
    {
        DiscordClient? Client { get; }
    }
}