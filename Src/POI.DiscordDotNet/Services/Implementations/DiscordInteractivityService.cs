using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;

namespace POI.DiscordDotNet.Services.Implementations;

public class DiscordInteractivityService : IAddDiscordClientFunctionality
{
	private readonly ILogger<DiscordInteractivityService> _logger;

	public DiscordInteractivityService(ILogger<DiscordInteractivityService> logger)
	{
		_logger = logger;
	}

	public Task Setup(IDiscordClientProvider discordClientProvider)
	{
		var client = discordClientProvider.Client!;
		client.UseInteractivity();

		return Task.CompletedTask;
	}
}