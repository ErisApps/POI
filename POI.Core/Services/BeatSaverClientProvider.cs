using System;
using BeatSaverSharp;

namespace POI.Core.Services
{
    public class BeatSaverClientProvider
    {
        private readonly BeatSaver _beatSaverClient;

        public BeatSaverClientProvider(string name, Version version)
        {
	        var beatSaverClientOptions = new HttpOptions(name, version, TimeSpan.FromSeconds(10), handleRateLimits: true);
	        _beatSaverClient = new BeatSaver(beatSaverClientOptions);
        }

        public BeatSaver GetClientInstance() => _beatSaverClient;
    }
}