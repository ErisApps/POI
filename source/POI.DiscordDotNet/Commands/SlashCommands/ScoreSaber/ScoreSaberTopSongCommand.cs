using Microsoft.Extensions.Logging;
using POI.DiscordDotNet.Services.Implementations;
using POI.Persistence.Repositories;
using POI.ThirdParty.BeatSaver.Services;
using POI.ThirdParty.ScoreSaber.Models.Wrappers;
using POI.ThirdParty.ScoreSaber.Services;

namespace POI.DiscordDotNet.Commands.SlashCommands.ScoreSaber;

public class ScoreSaberTopSongCommand : ScoreSaberBaseSongCommand
{
	public ScoreSaberTopSongCommand(ILogger<ScoreSaberTopSongCommand> logger,
		IScoreSaberApiService scoreSaberApiService,
		IGlobalUserSettingsRepository globalUserSettingsRepository,
		IBeatSaverClientProvider beatSaverClientProvider,
		PathProvider pathProvider)
		: base(logger, scoreSaberApiService, globalUserSettingsRepository, beatSaverClientProvider, pathProvider)
	{
	}

	protected override Task<PlayerScoresWrapperDto?> FetchScorePage(string playerId, uint page)
	{
		return ScoreSaberApiService.FetchTopSongsScorePage(playerId, page);
	}
}