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
using POI.DiscordDotNet.Services.Interfaces;

namespace POI.DiscordDotNet.Repositories
{
	public class GlobalUserSettingsRepository : BaseRepository<GlobalUserSettings>
	{
		public GlobalUserSettingsRepository(ILogger<GlobalUserSettingsRepository> logger, IMongoDbService mongoDbService) : base(logger, mongoDbService)
		{
		}

		internal Task<GlobalUserSettings?> LookupSettingsByDiscordId(string discordId) => FindOne(settings => settings.DiscordId == discordId);

		internal Task<GlobalUserSettings?> LookupSettingsByScoreSaberId(string scoreSaberId) => FindOne(settings => settings.AccountLinks.ScoreSaberId == scoreSaberId);

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

			var aggregationResultAsync = await GetCollection().AggregateAsync(aggregationPipelineDefinition).ConfigureAwait(false);
			return await aggregationResultAsync.ToListAsync().ConfigureAwait(false);
		}

		internal async Task CreateOrUpdateScoreSaberLink(string discordId, string scoreSaberId)
		{
			await CreateAndInsertIfNotExists(discordId);

			var updateDefinition = Builders<GlobalUserSettings>.Update.Set(settings => settings.AccountLinks.ScoreSaberId, scoreSaberId);
			await GetCollection().FindOneAndUpdateAsync(
				link => link.DiscordId == discordId,
				updateDefinition);
		}

		internal async Task<List<GlobalUserSettings>> GetAllBirthdayGirls(LocalDate birthdayDate)
		{
			var peopleWithRegisteredBirthday = await Find(settings => settings.Birthday != null);
			return peopleWithRegisteredBirthday
				.Where(settings => settings.Birthday!.Value.Day == birthdayDate.Day && settings.Birthday.Value.Month == birthdayDate.Month)
				.ToList();
		}

		internal async Task UpdateBirthday(string discordId, LocalDate? birthday)
		{
			await CreateAndInsertIfNotExists(discordId);

			var updateDefinition = Builders<GlobalUserSettings>.Update.Set(settings => settings.Birthday, birthday);
			await GetCollection().FindOneAndUpdateAsync(
				link => link.DiscordId == discordId,
				updateDefinition);
		}

		private async Task CreateAndInsertIfNotExists(string discordId)
		{
			var userSettings = await LookupSettingsByDiscordId(discordId).ConfigureAwait(false);
			if (userSettings == null)
			{
				userSettings = GlobalUserSettings.CreateDefault(discordId);
				await GetCollection().InsertOneAsync(userSettings).ConfigureAwait(false);
			}
		}

		internal async Task EnsureIndexes()
		{
			await EnsureIndexUniqueNullable("AccountLinks.ScoreSaberId", settings => settings.AccountLinks.ScoreSaberId!, BsonType.String);
			await EnsureIndexNonUnique("Birthday", settings => settings.Birthday!);
		}
	}
}