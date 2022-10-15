using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDb.Bson.NodaTime;
using MongoDB.Driver;
using POI.DiscordDotNet.Services.Interfaces;

namespace POI.DiscordDotNet.Services
{
	public class MongoDbService : IMongoDbService
	{
		private readonly ILogger<MongoDbService> _logger;
		private readonly ConfigProviderService _configProviderService;

		private readonly MongoClient _mongoClient;
		private readonly IMongoDatabase _mongoDatabase;

		public MongoDbService(ILogger<MongoDbService> logger, ConfigProviderService configProviderService, IConstants constants)
		{
			_logger = logger;
			_configProviderService = configProviderService;

			NodaTimeSerializers.Register();

			var mongoClientSettings = MongoClientSettings.FromConnectionString(_configProviderService.MongoDb.MongoDbConnectionString);
			mongoClientSettings.ApplicationName = $"{constants.Name}/{constants.Version.ToString(3)}";

			_logger.LogInformation("Connecting to MongoDb instance");
			_mongoClient = new MongoClient(mongoClientSettings);

			_mongoDatabase = _mongoClient.GetDatabase("POINext");
		}

		public IMongoCollection<T> GetCollection<T>(string? collectionName = null) where T : class
			=> _mongoDatabase.GetCollection<T>(collectionName ?? typeof(T).Name);

		public async Task<bool> TestConnectivity()
		{
			var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

			try
			{
				await _mongoDatabase.RunCommandAsync((Command<BsonDocument>) "{ping:1}", cancellationToken: cts.Token);
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}
	}
}