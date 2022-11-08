using BeatSaverSharp;
using POI.Core.Services;

namespace POI.ThirdParty.BeatSaver.Services.Implementations;

internal class BeatSaverClientProvider : IBeatSaverClientProvider
{
	private readonly BeatSaverSharp.BeatSaver _beatSaverClient;

	public BeatSaverClientProvider(IConstants constants)
	{
		var beatSaverClientOptions = new BeatSaverOptions(constants.Name, constants.Version)
		{
			Cache = false,
			Timeout = TimeSpan.FromSeconds(10)
		};
		_beatSaverClient = new BeatSaverSharp.BeatSaver(beatSaverClientOptions);
	}

	public BeatSaverSharp.BeatSaver GetClientInstance() => _beatSaverClient;
}