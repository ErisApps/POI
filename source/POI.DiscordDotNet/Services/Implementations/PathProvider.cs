using Microsoft.Extensions.Options;
using POI.DiscordDotNet.Configuration;

namespace POI.DiscordDotNet.Services.Implementations
{
	public class PathProvider
	{
		public PathProvider(IOptions<PathConfigurationOptions> options)
		{
			var baseDataPath = options.Value.DataFolderPath;
			AssetsPath = options.Value.AssetsFolderPath ?? Path.Combine(baseDataPath, "Assets");
			LogsPath = options.Value.LogsFolderPath ?? Path.Combine(baseDataPath, "Logs");
		}

		public string AssetsPath { get; }
		public string LogsPath { get; }
	}
}