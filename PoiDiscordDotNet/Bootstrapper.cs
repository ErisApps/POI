using System;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PoiDiscordDotNet.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace PoiDiscordDotNet
{
	internal static class Bootstrapper
	{
		private static Version? _version;
		private static DiscordClient _client = null!;

		internal static string Name { get; } = "POINext";
		internal static Version Version => _version ??= Assembly.GetExecutingAssembly().GetName().Version!;

		public static async Task Main(string[]? args = null)
		{
			var dockerized = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
			var pathProvider = new PathProvider(dockerized, args?.Length >= 1 ? args[0] : null);

			var logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.MinimumLevel.Override(nameof(DSharpPlus), LogEventLevel.Information)
				.Enrich.FromLogContext()
				.WriteTo.Console(theme: SystemConsoleTheme.Colored)
				.WriteTo.Conditional(_ => dockerized,
					(writeTo => writeTo.Async(
						writeToInternal => writeToInternal.File(pathProvider.LogsPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 60, buffered: true)
					)))
				.CreateLogger();

			var configProvider = new ConfigProviderService(logger.ForContext<ConfigProviderService>(), pathProvider.ConfigPath);
			if (!await configProvider.LoadConfig())
			{
				logger.Fatal("Exiting... Please ensure the config is correct.");
				return;
			}

			var seriFactory = new LoggerFactory().AddSerilog(logger.ForContext<DiscordClient>());
			_client = new DiscordClient(new DiscordConfiguration
			{
				Token = configProvider.Discord.Token,
				TokenType = TokenType.Bot,
				LoggerFactory = seriFactory,
				// This is apparently a bad idea according to the documentation... but I'm going to enable it regardless...
				ReconnectIndefinitely = true
			});

			var serviceProvider = new ServiceCollection()
				.AddLogging(loggingBuilderExtensions => loggingBuilderExtensions.AddSerilog(logger))
				.AddSingleton(configProvider)
				.AddSingleton(pathProvider)
				.AddSingleton(_client)
				.AddSingleton<MongoDbService>()
				.AddSingleton<UptimeManagementService>()
				.AddSingleton<BeatSaverClientProvider>()
				.AddSingleton<ScoreSaberService>()
				.BuildServiceProvider();

			// Verify mongoDbConnection
			if (!await VerifyMongoDbConnection(serviceProvider, logger))
			{
				return;
			}

			var commandsNext = _client.UseCommandsNext(new CommandsNextConfiguration
			{
				EnableMentionPrefix = false,
				EnableDefaultHelp = false,
				CaseSensitive = false,
				DmHelp = false,
				IgnoreExtraArguments = false,
				StringPrefixes = new[] {configProvider.Discord.Prefix},
				Services = serviceProvider
			});
			commandsNext.CommandErrored += (_, eventArgs) =>
			{
				logger.Error("{Username} tried to execute command {CommandName}, but it errored with message {ErrorMessage}", eventArgs.Context.User.Username, eventArgs.Command.Name, eventArgs.Exception);

				return Task.CompletedTask;
			};
			commandsNext.CommandExecuted += (_, eventArgs) =>
			{
				logger.Debug("{Username} executed command {CommandName}", eventArgs.Context.User.Username, eventArgs.Command.Name);

				return Task.CompletedTask;
			};
			commandsNext.RegisterCommands(Assembly.GetEntryAssembly());

			await _client.ConnectAsync(new DiscordActivity("POI for mod? (pretty please)", ActivityType.Playing)).ConfigureAwait(false);

			await Task.Delay(-1).ConfigureAwait(false);
		}

		private static async Task<bool> VerifyMongoDbConnection(IServiceProvider serviceProvider, Serilog.ILogger logger)
		{
			var mongoDbService = serviceProvider.GetService<MongoDbService>()!;
			if (await mongoDbService.TestConnectivity().ConfigureAwait(false))
			{
				logger.Information("Connected to MongoDb instance.");

				return true;
			}

			logger.Fatal("Couldn't connect to database. Exiting...");
			return false;
		}
	}
}