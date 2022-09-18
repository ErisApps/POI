using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using NodaTime;
using POI.DiscordDotNet.Models.AccountLink;
using POI.DiscordDotNet.Models.Database;

namespace POI.DiscordDotNet.Services
{
	public class GlobalUserSettingsService
	{
		private readonly ILogger<GlobalUserSettingsService> _logger;
		private readonly MongoDbService _mongoDbService;

		public GlobalUserSettingsService(ILogger<GlobalUserSettingsService> logger, MongoDbService mongoDbService)
		{
			_logger = logger;
			_mongoDbService = mongoDbService;
		}

		internal Task<GlobalUserSettings?> LookupSettingsByDiscordId(string discordId) => LookupUserSettingsOne(settings => settings.DiscordId == discordId);

		internal Task<GlobalUserSettings?> LookupSettingsByScoreSaberId(string scoreSaberId) => LookupUserSettingsOne(settings => settings.AccountLinks.ScoreSaberId == scoreSaberId);

		internal Task<List<ScoreSaberAccountLink>> GetAllScoreSaberAccountLinks() => GetAllAccountLinksGeneric(
			settings => settings.AccountLinks.ScoreSaberId != null,
			settings => new ScoreSaberAccountLink(settings.DiscordId, settings.AccountLinks.ScoreSaberId!));

		private async Task<List<TProjected>> GetAllAccountLinksGeneric<TProjected>(
			Expression<Func<GlobalUserSettings, bool>> genericAccountFilter,
			Expression<Func<GlobalUserSettings, TProjected>> genericAccountProjector) where TProjected : AccountLinkBase
		{
			var filterDefinition = Builders<GlobalUserSettings>.Filter.Where(genericAccountFilter);
			var projectionDefinition = Builders<GlobalUserSettings>.Projection.Expression(genericAccountProjector);

			var aggregationPipelineDefinition = new EmptyPipelineDefinition<GlobalUserSettings>()
				.AppendStage(PipelineStageDefinitionBuilder.Match(filterDefinition))
				.AppendStage(PipelineStageDefinitionBuilder.Project(projectionDefinition));

			var aggregationResultAsync = await GetUserSettingsCollection().AggregateAsync(aggregationPipelineDefinition).ConfigureAwait(false);
			return await aggregationResultAsync.ToListAsync().ConfigureAwait(false);
		}

		internal async Task CreateOrUpdateScoreSaberLink(string discordId, string scoreSaberId)
		{
			await CreateAndInsertUserSettingsIfNotExists(discordId);

			var updateDefinition = Builders<GlobalUserSettings>.Update.Set(settings => settings.AccountLinks.ScoreSaberId, scoreSaberId);
			await GetUserSettingsCollection().FindOneAndUpdateAsync(
				link => link.DiscordId == discordId,
				updateDefinition);
		}

		internal async Task<List<GlobalUserSettings>> GetAllBirthdayGirls(LocalDate birthdayDate)
		{
			var peopleWithRegisteredBirthday = await LookupUserSettings(settings => settings.Birthday != null);
			return peopleWithRegisteredBirthday
				.Where(settings => settings.Birthday!.Value.Day == birthdayDate.Day && settings.Birthday.Value.Month == birthdayDate.Month)
				.ToList();
		}

		internal async Task UpdateBirthday(string discordId, LocalDate? birthday)
		{
			await CreateAndInsertUserSettingsIfNotExists(discordId);

			var updateDefinition = Builders<GlobalUserSettings>.Update.Set(settings => settings.Birthday, birthday);
			await GetUserSettingsCollection().FindOneAndUpdateAsync(
				link => link.DiscordId == discordId,
				updateDefinition);
		}

		private async Task CreateAndInsertUserSettingsIfNotExists(string discordId)
		{
			var userSettings = await LookupSettingsByDiscordId(discordId).ConfigureAwait(false);
			if (userSettings == null)
			{
				userSettings = GlobalUserSettings.CreateDefault(discordId);
				await GetUserSettingsCollection().InsertOneAsync(userSettings).ConfigureAwait(false);
			}
		}

		private async Task<GlobalUserSettings?> LookupUserSettingsOne(Expression<Func<GlobalUserSettings, bool>> predicate)
		{
			try
			{
				var userSettings = await GetUserSettingsCollection()
					.FindAsync(new ExpressionFilterDefinition<GlobalUserSettings>(predicate))
					.ConfigureAwait(false);
				return userSettings.FirstOrDefault();
			}
			catch (Exception)
			{
				return null;
			}
		}

		private async Task<List<GlobalUserSettings>> LookupUserSettings(Expression<Func<GlobalUserSettings, bool>> predicate)
		{
			try
			{
				return await (await GetUserSettingsCollection()
						.FindAsync(new ExpressionFilterDefinition<GlobalUserSettings>(predicate))
						.ConfigureAwait(false))
					.ToListAsync().ConfigureAwait(false);
			}
			catch (Exception)
			{
				return new List<GlobalUserSettings>();
			}
		}

		private IMongoCollection<GlobalUserSettings> GetUserSettingsCollection()
		{
			return _mongoDbService.GetCollection<GlobalUserSettings>();
		}

		internal async Task EnsureIndexes()
		{
			await EnsureIndexUniqueNullable("AccountLinks.ScoreSaberId", settings => settings.AccountLinks.ScoreSaberId!, BsonType.String);
			await EnsureIndexNonUnique("Birthday", settings => settings.Birthday!);
		}

		private Task EnsureIndexNonUnique(string indexName, Expression<Func<GlobalUserSettings, object>> fieldSelector)
		{
			return EnsureSingleIndexInternal(indexName, fieldSelector);
		}

		private Task EnsureIndexUnique(string indexName, Expression<Func<GlobalUserSettings, object>> fieldSelector)
		{
			return EnsureSingleIndexInternal(indexName, fieldSelector, options =>
			{
				options.Unique = true;
			});
		}

		private Task EnsureIndexUniqueNullable(string indexName, Expression<Func<GlobalUserSettings, object>> fieldSelector, BsonType bsonFieldType)
		{
			return EnsureSingleIndexInternal(indexName, fieldSelector, options =>
			{
				options.Unique = true;
				options.PartialFilterExpression = Builders<GlobalUserSettings>.Filter.Type(fieldSelector, bsonFieldType);
			});
		}

		private async Task EnsureSingleIndexInternal(string indexName, Expression<Func<GlobalUserSettings, object>> fieldSelector,
			Action<CreateIndexOptions<GlobalUserSettings>>? indexCreationOptionsExtension = null)
		{
			var collectionIndexManager = GetUserSettingsCollection().Indexes;
			var collectionIndexesCursor = await collectionIndexManager.ListAsync().ConfigureAwait(false);
			var collectionIndexesList = await collectionIndexesCursor.ToListAsync().ConfigureAwait(false);
			if (collectionIndexesList.Any(x => x["name"] == indexName))
			{
				await collectionIndexManager.DropOneAsync(indexName).ConfigureAwait(false);
			}

			var indexKeysDefinition = Builders<GlobalUserSettings>.IndexKeys.Ascending(fieldSelector);
			var indexCreationOptions = new CreateIndexOptions<GlobalUserSettings> { Name = indexName };
			indexCreationOptionsExtension?.Invoke(indexCreationOptions);

			await collectionIndexManager.CreateOneAsync(new CreateIndexModel<GlobalUserSettings>(indexKeysDefinition, indexCreationOptions));
		}
	}
}