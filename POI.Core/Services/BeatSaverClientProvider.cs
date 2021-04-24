using System;
using BeatSaverSharp;
using POI.Core.Services.Interfaces;

namespace POI.Core.Services
{
	public class BeatSaverClientProvider
	{
		private readonly BeatSaver _beatSaverClient;

		public BeatSaverClientProvider(IConstantsCore constants)
		{
			var beatSaverClientOptions = new HttpOptions(constants.Name, constants.Version, TimeSpan.FromSeconds(10), handleRateLimits: true);
			_beatSaverClient = new BeatSaver(beatSaverClientOptions);
		}

		public BeatSaver GetClientInstance() => _beatSaverClient;
	}
}