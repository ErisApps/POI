using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;
using POI.DiscordDotNet.Commands.SlashCommands.Modules;

namespace POI.DiscordDotNet.Services.Implementations;

public class DiscordSlashCommandsService : IAddDiscordClientFunctionality
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<DiscordSlashCommandsService> _logger;

	private SlashCommandsExtension? _slashCommands;

	public DiscordSlashCommandsService(
		IServiceProvider serviceProvider,
		ILogger<DiscordSlashCommandsService> logger)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
	}

	public void Setup(IDiscordClientProvider discordClientProvider)
	{
		_logger.LogDebug("Setting up DiscordSlashCommandsService");

		var client = discordClientProvider.Client!;
		_slashCommands = client.GetSlashCommands() ?? client.UseSlashCommands(new SlashCommandsConfiguration { Services = _serviceProvider });

		_slashCommands.SlashCommandErrored -= OnSlashCommandErrored;
		_slashCommands.SlashCommandErrored += OnSlashCommandErrored;
		_slashCommands.SlashCommandExecuted -= OnSlashCommandsExecuted;
		_slashCommands.SlashCommandExecuted += OnSlashCommandsExecuted;

		// TODO: Register slash commands below
		_slashCommands.RegisterCommands<ProfileSlashCommandsModule>();
		_slashCommands.RegisterCommands<TestSlashCommandsModule>();
		_slashCommands.RegisterCommands<UtilSlashCommandsModule>();
	}

	public void Cleanup(IDiscordClientProvider discordClientProvider)
	{
		if (_slashCommands == null)
		{
			return;
		}

		_logger.LogDebug("Cleaning up DiscordSlashCommandsService");
		_slashCommands.SlashCommandErrored -= OnSlashCommandErrored;
		_slashCommands.SlashCommandExecuted -= OnSlashCommandsExecuted;
		_slashCommands = null;
	}

	private async Task OnSlashCommandErrored(SlashCommandsExtension _, SlashCommandErrorEventArgs eventArgs)
	{
		if (eventArgs.Exception is SlashExecutionChecksFailedException castedException)
		{
			var timeLeft = string.Empty;
			foreach (var error in castedException.FailedChecks)
			{
				var cooldown = (SlashCooldownAttribute) error;
				var rawTime = cooldown.GetRemainingCooldown(eventArgs.Context);

				if (rawTime.Days != 0) timeLeft += $"{rawTime.Days} days, ";
				if (rawTime.Hours != 0) timeLeft += $"{rawTime.Hours} hours, ";
				if (rawTime.Minutes != 0) timeLeft += $"{rawTime.Minutes} minutes, ";
				if (rawTime.Seconds != 0) timeLeft += $"{rawTime.Seconds} seconds";
			}

			await eventArgs.Context.CreateResponseAsync($"To use that command you need to wait {timeLeft}", true).ConfigureAwait(false);
			return;
		}

		_logger.LogError(eventArgs.Exception,
			"{Username} tried to execute slashcommand /{CommandName}, but it errored",
			eventArgs.Context.User.Username, eventArgs.Context.CommandName);
	}

	private Task OnSlashCommandsExecuted(SlashCommandsExtension _, SlashCommandExecutedEventArgs eventArgs)
	{
		_logger.LogDebug("{Username} executed slashcommand /{CommandName}",
			eventArgs.Context.User.Username,
			eventArgs.Context.CommandName);

		return Task.CompletedTask;
	}
}