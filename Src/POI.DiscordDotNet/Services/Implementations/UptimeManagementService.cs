using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Extensions;

namespace POI.DiscordDotNet.Services.Implementations
{
	public class UptimeManagementService : IAddDiscordClientFunctionality
	{
		private readonly ILogger<UptimeManagementService> _logger;

		public Instant? UpSince { get; private set; }

		public UptimeManagementService(ILogger<UptimeManagementService> logger)
		{
			_logger = logger;
		}

		public Task Setup(IDiscordClientProvider discordClientProvider)
		{
			var client = discordClientProvider.Client!;

			client.Ready -= ClientOnReady;
			client.Ready += ClientOnReady;

			return Task.CompletedTask;
		}

		private Task ClientOnReady(DiscordClient sender, ReadyEventArgs e)
		{
			_logger.LogDebug("Client OnReady event received. (Re)setting time since start");
			UpSince = DateTimeOffset.Now.ToInstant();

			return Task.CompletedTask;
		}
	}
}