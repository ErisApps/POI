using NodaTime;
using POI.DiscordDotNet.Persistence.Domain;
using POI.DiscordDotNet.Persistence.Models.AccountLink;

namespace POI.DiscordDotNet.Persistence.Repositories;

public interface IGlobalUserSettingsRepository
{
	Task<GlobalUserSettings?> LookupSettingsByDiscordId(ulong discordId, CancellationToken cts = default);
	Task<GlobalUserSettings?> LookupSettingsByScoreSaberId(string scoreSaberId, CancellationToken cts = default);
	Task<List<ScoreSaberAccountLink>> GetAllScoreSaberAccountLinks(CancellationToken cts = default);
	Task CreateOrUpdateScoreSaberLink(ulong discordId, string scoreSaberId, CancellationToken cts = default);

	Task<List<GlobalUserSettings>> GetAllBirthdayGirls(LocalDate birthdayDate, CancellationToken cts = default);
	Task UpdateBirthday(ulong discordId, LocalDate? birthday, CancellationToken cts = default);
}