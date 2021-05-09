namespace POI.DiscordDotNet.Models.Configuration
{
	public class Configuration
	{
		public DiscordConfig? DiscordConfig { get; init; }
		public MongoDbConfig? MongoDbConfig { get; init; }
		public ApiConfig? ApiConfig { get; init; }
	}
}