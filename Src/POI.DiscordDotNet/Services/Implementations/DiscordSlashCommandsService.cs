using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;
using POI.DiscordDotNet.Commands.Profile;
using POI.DiscordDotNet.Commands.Utils;

namespace POI.DiscordDotNet.Services.Implementations;

public class DiscordSlashCommandsService : IAddDiscordClientFunctionality, IDisposable
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

	public Task Setup(IDiscordClientProvider discordClientProvider)
	{
		var client = discordClientProvider.Client!;
		_slashCommands = client.GetSlashCommands() ?? client.UseSlashCommands(new SlashCommandsConfiguration { Services = _serviceProvider });

		_slashCommands.SlashCommandErrored -= OnSlashCommandErrored;
		_slashCommands.SlashCommandErrored += OnSlashCommandErrored;
		_slashCommands.SlashCommandExecuted -= OnSlashCommandsExecuted;
		_slashCommands.SlashCommandExecuted += OnSlashCommandsExecuted;

		// TODO: Register slash commands below
		_slashCommands.RegisterCommands<PingCommand>();
		_slashCommands.RegisterCommands<UptimeCommand>();

		_slashCommands.RegisterCommands<ProfileSlashCommandsModule>();

		return Task.CompletedTask;
	}

	public void Dispose()
	{
		if (_slashCommands == null)
		{
			return;
		}

		_slashCommands.SlashCommandErrored -= OnSlashCommandErrored;
		_slashCommands.SlashCommandExecuted -= OnSlashCommandsExecuted;
		_slashCommands = null;
	}

	private Task OnSlashCommandErrored(SlashCommandsExtension _, SlashCommandErrorEventArgs eventArgs)
	{
		_logger.LogError(eventArgs.Exception,
			"{Username} tried to execute slashcommand /{CommandName}, but it errored",
			eventArgs.Context.User.Username, eventArgs.Context.CommandName);

		return Task.CompletedTask;
	}

	private Task OnSlashCommandsExecuted(SlashCommandsExtension _, SlashCommandExecutedEventArgs eventArgs)
	{
		_logger.LogDebug("{Username} executed slashcommand /{CommandName}", eventArgs.Context.User.Username,
			eventArgs.Context.CommandName);

		return Task.CompletedTask;
	}
}