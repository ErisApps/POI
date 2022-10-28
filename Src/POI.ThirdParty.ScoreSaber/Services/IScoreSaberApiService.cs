using POI.ThirdParty.ScoreSaber.Models.Profile;
using POI.ThirdParty.ScoreSaber.Models.Scores;
using POI.ThirdParty.ScoreSaber.Models.Wrappers;

namespace POI.ThirdParty.ScoreSaber.Services
{
	public interface IScoreSaberApiService
	{
		Task<BasicProfile?> FetchBasicPlayerProfile(string scoreSaberId, CancellationToken cancellationToken = default);
		Task<FullProfile?> FetchFullPlayerProfile(string scoreSaberId, CancellationToken cancellationToken = default);
		Task<PlayerScoresWrapper?> FetchRecentSongsScorePage(string scoreSaberId, uint page, uint? limit = null, CancellationToken cancellationToken = default);
		IAsyncEnumerable<PlayerScore> FetchPlayerRecentScoresPaged(string scoreSaberId, uint? itemsPerPage = null, CancellationToken cancellationToken = default);
		Task<PlayerScoresWrapper?> FetchTopSongsScorePage(string scoreSaberId, uint page, uint? limit = null, CancellationToken cancellationToken = default);
		IAsyncEnumerable<PlayerScore> FetchPlayerTopScoresPaged(string scoreSaberId, uint? itemsPerPage = null, CancellationToken cancellationToken = default);
		Task<PlayerScoresWrapper?> FetchPlayerScores(string scoreSaberId, uint page, SortType sortType, uint? limit = null, CancellationToken cancellationToken = default);
		IAsyncEnumerable<PlayerScore> FetchPlayerScoresPaged(string scoreSaberId, SortType sortType, uint? itemsPerPage = null, CancellationToken cancellationToken = default);
		Task<PlayersWrapper?> FetchPlayers(uint page, string? searchQuery = null, string[]? countries = null, CancellationToken cancellationToken = default);
		IAsyncEnumerable<ExtendedBasicProfile> FetchPlayersPaged(string? searchQuery = null, string[]? countries = null, CancellationToken cancellationToken = default);
		Task<Refresh?> RefreshProfile(string scoreSaberId, CancellationToken cancellationToken = default);
		Task<byte[]?> FetchImageFromCdn(string url, CancellationToken cancellationToken = default);
	}
}