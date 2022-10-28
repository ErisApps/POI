using POI.ThirdParty.BeatSavior.Models;

namespace POI.ThirdParty.BeatSavior.Services
{
	public interface IBeatSaviorApiService
	{
		Task<List<SongData>?> FetchBeatSaviorPlayerData(string scoreSaberId);
	}
}