using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using POI.DiscordDotNet.Models.Database;
using POI.DiscordDotNet.Services.Interfaces;

namespace POI.DiscordDotNet.Repositories
{
	public class ServerSettingsRepository : BaseRepository<ServerSettings>
	{
		public ServerSettingsRepository(ILogger<ServerSettingsRepository> logger, IMongoDbService mongoDbService) : base(logger, mongoDbService)
		{
		}

		public Task<ServerSettings?> FindOneById(ulong serverId)
		{
			return FindOne(settings => settings.ServerId == serverId);
		}

		private async Task CreateAndInsertIfNotExists(ulong userId, ulong serverId)
		{
			var serverSettings = await FindOneById(serverId).ConfigureAwait(false);
			if (serverSettings == null)
			{
				serverSettings = ServerSettings.CreateDefault(serverId);
				await GetCollection().InsertOneAsync(serverSettings).ConfigureAwait(false);
			}
		}
	}
}