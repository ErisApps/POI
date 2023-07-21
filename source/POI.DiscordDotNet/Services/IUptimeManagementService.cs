using NodaTime;

namespace POI.DiscordDotNet.Services;

public interface IUptimeManagementService
{
	Instant? UpSince { get; }
}