using System;
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

		internal async Task<string?> LookupScoreSaberId(string discordId)
		{
			return (await LookupScoreSaberLink(link => link.DiscordId == discordId).ConfigureAwait(false))?.ScoreSaberId;
		}

		internal async Task<string?> LookupDiscordId(string scoreSaberId)
		{
			return (await LookupScoreSaberLink(link => link.ScoreSaberId == scoreSaberId).ConfigureAwait(false))?.DiscordId;
		}

		internal Task CreateScoreSaberLink(string discordId, string scoreSaberId)
		{
			return GetScoreSaberLinkCollection().InsertOneAsync(new ScoreSaberLink(discordId, scoreSaberId));
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