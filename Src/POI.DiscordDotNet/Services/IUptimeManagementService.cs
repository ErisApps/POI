using NodaTime;

namespace POI.DiscordDotNet.Services.Implementations;

public interface IUptimeManagementService
{
	Instant? UpSince { get; }
}