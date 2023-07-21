using POI.ThirdParty.ScoreSaber.Models.Profile;
using POI.ThirdParty.ScoreSaber.Models.Scores;
using POI.ThirdParty.ScoreSaber.Models.Wrappers;

namespace POI.ThirdParty.ScoreSaber.Services;

public interface IScoreSaberApiService
{
	Task<BasicProfileDto?> FetchBasicPlayerProfile(string scoreSaberId, CancellationToken cancellationToken = default);
	Task<FullProfileDto?> FetchFullPlayerProfile(string scoreSaberId, CancellationToken cancellationToken = default);
	Task<PlayerScoresWrapperDto?> FetchRecentSongsScorePage(string scoreSaberId, uint page, uint? limit = null, CancellationToken cancellationToken = default);
	IAsyncEnumerable<PlayerScoreDto> FetchPlayerRecentScoresPaged(string scoreSaberId, uint? itemsPerPage = null, CancellationToken cancellationToken = default);
	Task<PlayerScoresWrapperDto?> FetchTopSongsScorePage(string scoreSaberId, uint page, uint? limit = null, CancellationToken cancellationToken = default);
	IAsyncEnumerable<PlayerScoreDto> FetchPlayerTopScoresPaged(string scoreSaberId, uint? itemsPerPage = null, CancellationToken cancellationToken = default);
	Task<PlayerScoresWrapperDto?> FetchPlayerScores(string scoreSaberId, uint page, SortType sortType, uint? limit = null, CancellationToken cancellationToken = default);
	IAsyncEnumerable<PlayerScoreDto> FetchPlayerScoresPaged(string scoreSaberId, SortType sortType, uint? itemsPerPage = null, CancellationToken cancellationToken = default);
	Task<PlayersWrapperDto?> FetchPlayers(uint page, string? searchQuery = null, string[]? countries = null, CancellationToken cancellationToken = default);
	IAsyncEnumerable<ExtendedBasicProfileDto> FetchPlayersPaged(string? searchQuery = null, string[]? countries = null, CancellationToken cancellationToken = default);
	Task<RefreshDto?> RefreshProfile(string scoreSaberId, CancellationToken cancellationToken = default);
	Task<byte[]?> FetchImageFromCdn(string url, CancellationToken cancellationToken = default);
}