using NodaTime;
using POI.Persistence.Domain;
using POI.Persistence.Models.AccountLink;

namespace POI.Persistence.Repositories;

public interface IGlobalUserSettingsRepository
{
	Task<GlobalUserSettings?> LookupSettingsByDiscordId(ulong discordUserId, CancellationToken cts = default);
	Task<GlobalUserSettings?> LookupSettingsByScoreSaberId(string scoreSaberId, CancellationToken cts = default);
	Task<List<ScoreSaberAccountLink>> GetAllScoreSaberAccountLinks(CancellationToken cts = default);
	Task CreateOrUpdateScoreSaberLink(ulong discordUserId, string scoreSaberId, CancellationToken cts = default);
	Task CreateOrUpdateBeatLeaderLink(ulong discordUserId, string beatLeaderId, CancellationToken cts = default);
	Task CreateOrUpdateBeatSaverLink(ulong discordUserId, string beatSaverId, CancellationToken cts = default);
	Task<List<GlobalUserSettings>> GetAllBirthdayGirls(int dayOfMonth, int month, CancellationToken cts = default);
	Task UpdateBirthday(ulong discordUserId, LocalDate? birthday, CancellationToken cts = default);
}