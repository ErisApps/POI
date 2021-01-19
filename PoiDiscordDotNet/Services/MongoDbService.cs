using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace PoiDiscordDotNet.Services
{
	public class MongoDbService
	{
		private readonly ILogger<MongoDbService> _logger;
		private readonly ConfigProviderService _configProviderService;

		private readonly MongoClient _mongoClient;
		private readonly IMongoDatabase _mongoDatabase;

		public MongoDbService(ILogger<MongoDbService> logger, ConfigProviderService configProviderService)
		{
			_logger = logger;
			_configProviderService = configProviderService;

			var mongoClientSettings = MongoClientSettings.FromConnectionString(_configProviderService.MongoDb.MongoDbConnectionString);
			mongoClientSettings.ApplicationName = $"{Bootstrapper.Name}/{Bootstrapper.Version.ToString(3)}";

			_logger.LogInformation("Connecting to MongoDb instance.");
			_mongoClient = new MongoClient(mongoClientSettings);
			_logger.LogInformation("Connected to MongoDb instance.");

			_mongoDatabase = _mongoClient.GetDatabase("POINext");
		}

		internal IMongoCollection<T> GetCollection<T>(string? collectionName = null) where T : class, new()
			=> _mongoDatabase.GetCollection<T>(collectionName ?? typeof(T).Name);
	}
}