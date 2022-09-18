using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using POI.DiscordDotNet.Services.Interfaces;

namespace POI.DiscordDotNet.Repositories
{
	public class BaseRepository<TDatabaseModel> where TDatabaseModel : class
	{
		protected readonly ILogger Logger;
		protected readonly IMongoDbService MongoDbService;

		public BaseRepository(ILogger logger, IMongoDbService mongoDbService)
		{
			Logger = logger;
			MongoDbService = mongoDbService;
		}

		protected IMongoCollection<TDatabaseModel> GetCollection() => MongoDbService.GetCollection<TDatabaseModel>();

		protected internal virtual async Task EnsureIndexes()
		{
		}

		public async Task<TDatabaseModel?> FindOne(Expression<Func<TDatabaseModel, bool>> predicate)
		{
			try
			{
				var userSettings = await GetCollection()
					.FindAsync(new ExpressionFilterDefinition<TDatabaseModel>(predicate))
					.ConfigureAwait(false);
				return userSettings.FirstOrDefault();
			}
			catch (Exception)
			{
				return null;
			}
		}

		public async Task<List<TDatabaseModel>> Find(Expression<Func<TDatabaseModel, bool>> predicate)
		{
			try
			{
				return await (await GetCollection()
						.FindAsync(new ExpressionFilterDefinition<TDatabaseModel>(predicate))
						.ConfigureAwait(false))
					.ToListAsync().ConfigureAwait(false);
			}
			catch (Exception)
			{
				return new List<TDatabaseModel>();
			}
		}

		protected Task EnsureIndexNonUnique(string indexName, Expression<Func<TDatabaseModel, object>> fieldSelector)
		{
			return EnsureSingleIndexInternal(indexName, fieldSelector);
		}

		protected Task EnsureIndexUnique(string indexName, Expression<Func<TDatabaseModel, object>> fieldSelector)
		{
			return EnsureSingleIndexInternal(indexName, fieldSelector, options =>
			{
				options.Unique = true;
			});
		}

		protected Task EnsureIndexUniqueNullable(string indexName, Expression<Func<TDatabaseModel, object>> fieldSelector, BsonType bsonFieldType)
		{
			return EnsureSingleIndexInternal(indexName, fieldSelector, options =>
			{
				options.Unique = true;
				options.PartialFilterExpression = Builders<TDatabaseModel>.Filter.Type(fieldSelector, bsonFieldType);
			});
		}

		private async Task EnsureSingleIndexInternal(string indexName, Expression<Func<TDatabaseModel, object>> fieldSelector,
			Action<CreateIndexOptions<TDatabaseModel>>? indexCreationOptionsExtension = null)
		{
			var collectionIndexManager = GetCollection().Indexes;
			var collectionIndexesCursor = await collectionIndexManager.ListAsync().ConfigureAwait(false);
			var collectionIndexesList = await collectionIndexesCursor.ToListAsync().ConfigureAwait(false);
			if (collectionIndexesList.Any(x => x["name"] == indexName))
			{
				await collectionIndexManager.DropOneAsync(indexName).ConfigureAwait(false);
			}

			var indexKeysDefinition = Builders<TDatabaseModel>.IndexKeys.Ascending(fieldSelector);
			var indexCreationOptions = new CreateIndexOptions<TDatabaseModel> { Name = indexName };
			indexCreationOptionsExtension?.Invoke(indexCreationOptions);

			await collectionIndexManager.CreateOneAsync(new CreateIndexModel<TDatabaseModel>(indexKeysDefinition, indexCreationOptions));
		}
	}
}