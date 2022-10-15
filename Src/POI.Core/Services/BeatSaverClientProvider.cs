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
			var beatSaverClientOptions = new BeatSaverOptions(constants.Name, constants.Version)
			{
				Cache = false,
				Timeout = TimeSpan.FromSeconds(10)
			};
			_beatSaverClient = new BeatSaver(beatSaverClientOptions);
		}

		public BeatSaver GetClientInstance() => _beatSaverClient;
	}
}