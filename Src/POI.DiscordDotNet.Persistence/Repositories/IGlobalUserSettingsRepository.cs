using NodaTime;
using POI.DiscordDotNet.Persistence.Domain;

namespace POI.DiscordDotNet.Persistence.Repositories;

public interface IGlobalUserSettingsRepository
{
	Task<GlobalUserSettings?> LookupSettingsByDiscordId(ulong discordId, CancellationToken cts = default);
	Task<GlobalUserSettings?> LookupSettingsByScoreSaberId(string scoreSaberId, CancellationToken cts = default);

	// Task<List<ScoreSaberAccountLink>> GetAllScoreSaberAccountLinks();

	Task<List<GlobalUserSettings>> GetAllBirthdayGirls(LocalDate birthdayDate, CancellationToken cts = default);
	Task UpdateBirthday(ulong discordId, LocalDate? birthday, CancellationToken cts = default);
}