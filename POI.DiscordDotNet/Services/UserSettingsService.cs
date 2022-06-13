using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
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

		internal Task<UserSettings?> LookupSettingsByDiscordId(string discordId) => LookupUserSettings(settings => settings.DiscordId == discordId);

		internal Task<UserSettings?> LookupSettingsByScoreSaberId(string scoreSaberId) => LookupUserSettings(settings => settings.AccountLinks.ScoreSaberId == scoreSaberId);

		internal async Task CreateOrUpdateScoreSaberLink(string discordId, string scoreSaberId)
		{
			await CreateAndInsertUserSettingsIfNotExists(discordId);

			var updateDefinition = Builders<UserSettings>.Update.Set(settings => settings.AccountLinks.ScoreSaberId, scoreSaberId);
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

		private async Task<UserSettings?> LookupUserSettings(Expression<Func<UserSettings, bool>> predicate)
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

		private IMongoCollection<UserSettings> GetUserSettingsCollection()
		{
			return _mongoDbService.GetCollection<UserSettings>();
		}
	}
}