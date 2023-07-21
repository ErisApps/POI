using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using POI.DiscordDotNet.Configuration;

namespace POI.DiscordDotNet.Services.Implementations;

public class DiscordChatCommandsService : IAddDiscordClientFunctionality
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<DiscordChatCommandsService> _logger;
	private readonly IOptions<DiscordConfigurationOptions> _options;

	private CommandsNextExtension? _commandsNext;

	public DiscordChatCommandsService(
		IServiceProvider serviceProvider,
		ILogger<DiscordChatCommandsService> logger,
		IOptions<DiscordConfigurationOptions> options)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
		_options = options;
	}

	public void Setup(IDiscordClientProvider discordClientProvider)
	{
		_logger.LogDebug("Setting up DiscordChatCommandsService");

		var client = discordClientProvider.Client!;
		_commandsNext = client.UseCommandsNext(new CommandsNextConfiguration
		{
			EnableMentionPrefix = false,
			EnableDefaultHelp = false,
			CaseSensitive = false,
			DmHelp = false,
			IgnoreExtraArguments = false,
			StringPrefixes = new[] { _options.Value.Prefix },
			Services = _serviceProvider
		});

		_commandsNext.CommandExecuted -= OnCommandsNextOnCommandExecuted;
		_commandsNext.CommandExecuted += OnCommandsNextOnCommandExecuted;
		_commandsNext.CommandErrored -= OnCommandsNextOnCommandErrored;
		_commandsNext.CommandErrored += OnCommandsNextOnCommandErrored;

		_commandsNext.RegisterCommands(typeof(DiscordChatCommandsService).Assembly);
	}

	public void Cleanup(IDiscordClientProvider discordClientProvider)
	{
		if (_commandsNext == null)
		{
			return;
		}

		_logger.LogDebug("Cleaning up DiscordChatCommandsService");
		_commandsNext.CommandExecuted -= OnCommandsNextOnCommandExecuted;
		_commandsNext.CommandErrored -= OnCommandsNextOnCommandErrored;
		_commandsNext = null;
	}

	private Task OnCommandsNextOnCommandExecuted(CommandsNextExtension _, CommandExecutionEventArgs eventArgs)
	{
		_logger.LogDebug("{Username} executed command {CommandName}", eventArgs.Context.User.Username, eventArgs.Command.Name);

		return Task.CompletedTask;
	}

	private Task OnCommandsNextOnCommandErrored(CommandsNextExtension _, CommandErrorEventArgs eventArgs)
	{
		_logger.LogError(eventArgs.Exception, "{Username} tried to execute command {CommandName}, but it errored", eventArgs.Context.User.Username, eventArgs.Command?.Name);

		return Task.CompletedTask;
	}
}