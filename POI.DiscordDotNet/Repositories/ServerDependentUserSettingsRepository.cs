using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using POI.DiscordDotNet.Models.Database;
using POI.DiscordDotNet.Services.Interfaces;

namespace POI.DiscordDotNet.Repositories
{
	public class ServerDependentUserSettingsRepository : BaseRepository<ServerDependentUserSettings>
	{
		public ServerDependentUserSettingsRepository(ILogger<ServerDependentUserSettingsRepository> logger, IMongoDbService mongoDbService) : base(logger, mongoDbService)
		{
		}

		public Task<ServerDependentUserSettings?> FindOneById(ulong userId, ulong serverId)
		{
			return FindOne(settings => settings.UserId == userId && settings.ServerId == serverId);
		}

		internal async Task UpdatePermissions(ulong userId, ulong serverId, Permissions newPermissions)
		{
			await CreateAndInsertIfNotExists(userId, serverId);

			var updateDefinition = Builders<ServerDependentUserSettings>.Update.Set(settings => settings.Permissions, newPermissions);
			await GetCollection().FindOneAndUpdateAsync(
				settings => settings.UserId == userId && settings.ServerId == serverId,
				updateDefinition);
		}

		protected internal override Task EnsureIndexes()
		{
			return EnsureSingleIndex("_compoundId",
				() => Builders<ServerDependentUserSettings>.IndexKeys.Combine(new List<IndexKeysDefinition<ServerDependentUserSettings>>
				{
					Builders<ServerDependentUserSettings>.IndexKeys.Ascending(x => x.UserId),
					Builders<ServerDependentUserSettings>.IndexKeys.Ascending(x => x.ServerId)
				}),
				options => options.Unique = true);
		}

		private async Task CreateAndInsertIfNotExists(ulong userId, ulong serverId)
		{
			var serverDependentUserSettings = await FindOneById(userId, serverId).ConfigureAwait(false);
			if (serverDependentUserSettings == null)
			{
				serverDependentUserSettings = ServerDependentUserSettings.CreateDefault(userId, serverId);
				await GetCollection().InsertOneAsync(serverDependentUserSettings).ConfigureAwait(false);
			}
		}
	}
}