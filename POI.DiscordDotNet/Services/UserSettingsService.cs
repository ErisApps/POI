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