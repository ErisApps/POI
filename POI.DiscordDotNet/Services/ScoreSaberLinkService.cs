using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using POI.DiscordDotNet.Models.Database;

namespace POI.DiscordDotNet.Services
{
	public class ScoreSaberLinkService
	{
		private readonly MongoDbService _mongoDbService;
		private readonly ILogger<ScoreSaberLinkService> _logger;

		public ScoreSaberLinkService(ILogger<ScoreSaberLinkService> logger, MongoDbService mongoDbService)
		{
			_logger = logger;
			_mongoDbService = mongoDbService;
		}

		internal Task<ScoreSaberLink?> LookupLinkByDiscordId(string discordId) => LookupScoreSaberLink(link => link.DiscordId == discordId);

		internal Task<ScoreSaberLink?> LookupLinkByScoreSaberId(string scoreSaberId) => LookupScoreSaberLink(link => link.ScoreSaberId == scoreSaberId);

		internal async Task<string?> LookupScoreSaberId(string discordId) => (await LookupLinkByDiscordId(discordId).ConfigureAwait(false))?.ScoreSaberId;

		internal async Task<string?> LookupDiscordId(string scoreSaberId) => (await LookupLinkByScoreSaberId(scoreSaberId).ConfigureAwait(false))?.DiscordId;

		internal async Task<List<ScoreSaberLink>> GetAll() => await (await GetScoreSaberLinkCollection().FindAsync(_ => true)).ToListAsync();

		internal Task CreateOrUpdateScoreSaberLink(string discordId, string scoreSaberId)
		{
			return GetScoreSaberLinkCollection().ReplaceOneAsync(
				link => link.DiscordId == discordId,
				new ScoreSaberLink(discordId, scoreSaberId),
				new ReplaceOptions
				{
					IsUpsert = true
				});
		}

		private async Task<ScoreSaberLink?> LookupScoreSaberLink(Expression<Func<ScoreSaberLink, bool>> predicate)
		{
			try
			{
				var userScoreLinks = await GetScoreSaberLinkCollection()
					.FindAsync(new ExpressionFilterDefinition<ScoreSaberLink>(predicate))
					.ConfigureAwait(false);
				return userScoreLinks.FirstOrDefault();
			}
			catch (Exception)
			{
				return null;
			}
		}

		private IMongoCollection<ScoreSaberLink> GetScoreSaberLinkCollection()
		{
			return _mongoDbService.GetCollection<ScoreSaberLink>();
		}
	}
}