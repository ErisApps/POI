using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NodaTime;
using POI.DiscordDotNet.Models.AccountLink;
using POI.DiscordDotNet.Models.Database;

namespace POI.DiscordDotNet.Services
{
	public class UserSettingsService
	{
		private readonly ILogger<UserSettingsService> _logger;
		private readonly MongoDbService _mongoDbService;

		public UserSettingsService(ILogger<UserSettingsService> logger, MongoDbService mongoDbService)
		{
			_logger = logger;
			_mongoDbService = mongoDbService;
		}

		internal Task<UserSettings?> LookupSettingsByDiscordId(string discordId) => LookupUserSettingsOne(settings => settings.DiscordId == discordId);

		internal Task<UserSettings?> LookupSettingsByScoreSaberId(string scoreSaberId) => LookupUserSettingsOne(settings => settings.AccountLinks.ScoreSaberId == scoreSaberId);


		internal Task<List<ScoreSaberAccountLink>> GetAllScoreSaberAccountLinks() => GetAllAccountLinksGeneric(
			settings => settings.AccountLinks.ScoreSaberId != null,
			settings => new ScoreSaberAccountLink(settings.DiscordId, settings.AccountLinks.ScoreSaberId!));

		private async Task<List<TProjected>> GetAllAccountLinksGeneric<TProjected>(
			Expression<Func<UserSettings, bool>> genericAccountFilter,
			Expression<Func<UserSettings, TProjected>> genericAccountProjector) where TProjected : AccountLinkBase
		{
			var filterDefinition = Builders<UserSettings>.Filter.Where(genericAccountFilter);
			var projectionDefinition = Builders<UserSettings>.Projection.Expression(genericAccountProjector);

			var aggregationPipelineDefinition = new EmptyPipelineDefinition<UserSettings>()
				.AppendStage(PipelineStageDefinitionBuilder.Match(filterDefinition))
				.AppendStage(PipelineStageDefinitionBuilder.Project(projectionDefinition));

			var aggregationResultAsync = await GetUserSettingsCollection().AggregateAsync(aggregationPipelineDefinition).ConfigureAwait(false);
			return await aggregationResultAsync.ToListAsync().ConfigureAwait(false);
		}

		internal async Task CreateOrUpdateScoreSaberLink(string discordId, string scoreSaberId)
		{
			await CreateAndInsertUserSettingsIfNotExists(discordId);

			var updateDefinition = Builders<UserSettings>.Update.Set(settings => settings.AccountLinks.ScoreSaberId, scoreSaberId);
			await GetUserSettingsCollection().FindOneAndUpdateAsync(
				link => link.DiscordId == discordId,
				updateDefinition);
		}

		internal async Task<List<UserSettings>> GetAllBirthdayGirls(LocalDate birthdayDate)
		{
			var peopleWithRegisteredBirthday = await LookupUserSettings(settings => settings.Birthday != null);
			return peopleWithRegisteredBirthday
				.Where(settings => settings.Birthday!.Value.Day == birthdayDate.Day && settings.Birthday.Value.Month == birthdayDate.Month)
				.ToList();
		}

		internal async Task UpdateBirthday(string discordId, LocalDate? birthday)
		{
			await CreateAndInsertUserSettingsIfNotExists(discordId);

			var updateDefinition = Builders<UserSettings>.Update.Set(settings => settings.Birthday, birthday);
			await GetUserSettingsCollection().FindOneAndUpdateAsync(
				link => link.DiscordId == discordId,
				updateDefinition);
		}

		private async Task CreateAndInsertUserSettingsIfNotExists(string discordId)
		{
			var userSettings = await LookupSettingsByDiscordId(discordId).ConfigureAwait(false);
			if (userSettings == null)
			{
				userSettings = UserSettings.CreateDefault(discordId);
				await GetUserSettingsCollection().InsertOneAsync(userSettings).ConfigureAwait(false);
			}
		}

		private async Task<UserSettings?> LookupUserSettingsOne(Expression<Func<UserSettings, bool>> predicate)
		{
			try
			{
				var userSettings = await GetUserSettingsCollection()
					.FindAsync(new ExpressionFilterDefinition<UserSettings>(predicate))
					.ConfigureAwait(false);
				return userSettings.FirstOrDefault();
			}
			catch (Exception)
			{
				return null;
			}
		}

		private async Task<List<UserSettings>> LookupUserSettings(Expression<Func<UserSettings, bool>> predicate)
		{
			try
			{
				return await (await GetUserSettingsCollection()
					.FindAsync(new ExpressionFilterDefinition<UserSettings>(predicate))
						.ConfigureAwait(false))
					.ToListAsync().ConfigureAwait(false);
			}
			catch (Exception)
			{
				return new List<UserSettings>();
			}
		}

		private IMongoCollection<UserSettings> GetUserSettingsCollection()
		{
			return _mongoDbService.GetCollection<UserSettings>();
		}

		internal async Task EnsureIndexes()
		{
			await EnsureIndexInternal("AccountLinks.ScoreSaberId", settings => settings.AccountLinks.ScoreSaberId!);
			await EnsureIndexInternal("Birthday", settings => settings.Birthday!, false);
		}

		private async Task EnsureIndexInternal(string indexName, Expression<Func<UserSettings, object>> fieldSelector, bool unique = true)
		{
			var scoreSaberIdIndex = Builders<UserSettings>.IndexKeys.Ascending(fieldSelector);
			await GetUserSettingsCollection().Indexes
				.CreateOneAsync(new CreateIndexModel<UserSettings>(scoreSaberIdIndex, new CreateIndexOptions { Name = indexName, Unique = unique }));
		}
	}
}