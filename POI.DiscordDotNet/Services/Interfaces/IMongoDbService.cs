using System.Threading.Tasks;
using MongoDB.Driver;

namespace POI.DiscordDotNet.Services.Interfaces
{
	public interface IMongoDbService
	{
		IMongoCollection<T> GetCollection<T>(string? collectionName = null) where T : class;
		Task<bool> TestConnectivity();
	}
}