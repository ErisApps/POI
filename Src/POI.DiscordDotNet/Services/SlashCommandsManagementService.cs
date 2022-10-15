using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;
using POI.DiscordDotNet.Commands.Profile;
using POI.DiscordDotNet.Commands.Utils;

namespace POI.DiscordDotNet.Services
{
	public class SlashCommandsManagementService : IDisposable
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<SlashCommandsManagementService> _logger;
		private readonly DiscordClient _discordClient;

		private SlashCommandsExtension? _slashCommands;

		public SlashCommandsManagementService(IServiceProvider serviceProvider, ILogger<SlashCommandsManagementService> logger, DiscordClient discordClient)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
			_discordClient = discordClient;
		}

		public void Setup()
		{
			_slashCommands = _discordClient.UseSlashCommands(new SlashCommandsConfiguration { Services = _serviceProvider });
			_slashCommands.SlashCommandErrored += OnSlashCommandErrored;
			_slashCommands.SlashCommandExecuted += OnSlashCommandsExecuted;

			// TODO: Register slash commands below
			_slashCommands.RegisterCommands<PingCommand>();
			_slashCommands.RegisterCommands<UptimeCommand>();

			_slashCommands.RegisterCommands<ProfileSlashCommandsModule>();
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
			_logger.LogError(eventArgs.Exception, "{Username} tried to execute slashcommand /{CommandName}, but it errored", eventArgs.Context.User.Username, eventArgs.Context.CommandName);

			return Task.CompletedTask;
		}

		private Task OnSlashCommandsExecuted(SlashCommandsExtension _, SlashCommandExecutedEventArgs eventArgs)
		{
			_logger.LogDebug("{Username} executed slashcommand /{CommandName}", eventArgs.Context.User.Username, eventArgs.Context.CommandName);

			return Task.CompletedTask;
		}
	}
}