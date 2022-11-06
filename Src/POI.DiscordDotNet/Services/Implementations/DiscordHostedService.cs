using DSharpPlus.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace POI.DiscordDotNet.Services.Implementations;

public class DiscordHostedService : IHostedService
{
	private readonly ILogger<DiscordHostedService> _logger;
	private readonly IManageDiscordClientProvider _discordClientProvider;
	private readonly IAddDiscordClientFunctionality[] _discordClientFunctionalityEnrichers;

	public DiscordHostedService(
		ILogger<DiscordHostedService> logger,
		IManageDiscordClientProvider discordClientProvider,
		IEnumerable<IAddDiscordClientFunctionality> discordClientFunctionalityEnrichers)
	{
		_logger = logger;
		_discordClientProvider = discordClientProvider;
		_discordClientFunctionalityEnrichers = discordClientFunctionalityEnrichers.ToArray();
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		_discordClientProvider.Initialize();

		foreach (var clientFunctionalityEnricher in _discordClientFunctionalityEnrichers)
		{
			clientFunctionalityEnricher.Setup(_discordClientProvider);
		}

		_logger.LogInformation("Starting Discord client");
		await _discordClientProvider.Client!.ConnectAsync(new DiscordActivity("POI for mod? (pretty please)", ActivityType.Playing)).ConfigureAwait(false);
		_logger.LogInformation("Discord client started");
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		if (_discordClientProvider.Client == null)
		{
			_logger.LogWarning("Discord client is not initialized, can't be stopped");
			return;
		}

		foreach (var clientFunctionalityEnricher in _discordClientFunctionalityEnrichers)
		{
			clientFunctionalityEnricher.Cleanup(_discordClientProvider);
		}

		_logger.LogInformation("Stopping Discord client");
		await _discordClientProvider.Client.DisconnectAsync().ConfigureAwait(false);
		_logger.LogInformation("Discord client stopped");

		_discordClientProvider.Cleanup();
	}
}