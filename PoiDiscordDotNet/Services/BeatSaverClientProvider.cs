using System;
using BeatSaverSharp;

namespace PoiDiscordDotNet.Services
{
    public class BeatSaverClientProvider
    {
        private readonly BeatSaver _beatSaverClient;

        public BeatSaverClientProvider()
        {
	        var beatSaverClientOptions = new HttpOptions(Bootstrapper.Name, Bootstrapper.Version, TimeSpan.FromSeconds(10), handleRateLimits: true);
	        _beatSaverClient = new BeatSaver(beatSaverClientOptions);
        }

        public BeatSaver GetClientInstance() => _beatSaverClient;
    }
}