using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Extensions;

namespace POI.DiscordDotNet.Services
{
	public class UptimeManagementService
    {
        private readonly ILogger<UptimeManagementService> _logger;

        public Instant? UpSince { get; private set; }

        public UptimeManagementService(ILogger<UptimeManagementService> logger, DiscordClient client)
        {
            _logger = logger;

            client.Ready += ClientOnReady;
        }

        private Task ClientOnReady(DiscordClient sender, ReadyEventArgs e)
        {
            _logger.LogDebug("Client OnReady event received. (Re)setting time since start");
            UpSince = DateTimeOffset.Now.ToInstant();

            return Task.CompletedTask;
        }
    }
}