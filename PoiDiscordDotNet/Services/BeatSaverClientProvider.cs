using System;
using BeatSaverSharp;

namespace PoiDiscordDotNet.Services
{
    public class BeatSaverClientProvider
    {
        private readonly BeatSaver _beatSaverClient;

        public BeatSaverClientProvider()
        {
            _beatSaverClient = new BeatSaver(new HttpOptions
            {
                ApplicationName = Bootstrapper.Name,
                Version = Bootstrapper.Version,
                Timeout = TimeSpan.FromSeconds(10),
                HandleRateLimits = true
            });
        }
    }
}