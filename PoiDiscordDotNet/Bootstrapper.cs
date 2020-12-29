using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PoiDiscordDotNet.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace PoiDiscordDotNet
{
	class Bootstrapper
	{
		private static Version? _version;
		private static DiscordClient _client = null!;

		internal static string Name { get; } = "POINext";
		internal static Version Version => _version ??= Assembly.GetExecutingAssembly().GetName().Version!;

		public static async Task Main(string[] args)
		{
			string? dataPath;
			var dockerized = Environment.GetEnvironmentVariable("containerized") == "true";
			if (dockerized)
			{
				dataPath = "/data";
			}
			else if (args.Length >= 1 && !string.IsNullOrWhiteSpace(args[0]))
			{
				dataPath = args[0];
			}
			else
			{
				throw new ArgumentException("When running in the non-containerized mode. Please ensure that you're passing a dataPath as a launch argument.", nameof(args));
			}

			var logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.MinimumLevel.Override(nameof(DSharpPlus), LogEventLevel.Information)
				.Enrich.FromLogContext()
				.WriteTo.Console(theme: AnsiConsoleTheme.Code)
				.WriteTo.Conditional(_ => dockerized,
					(writeTo => writeTo.Async(
						writeToInternal => writeToInternal.File(Path.Combine(dataPath!, "logs", "log.txt"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 60)
					)))
				.CreateLogger();

			var configProvider = new ConfigProviderService(logger.ForContext<ConfigProviderService>(), Path.Combine(dataPath, ConfigProviderService.CONFIG_FILE_NAME));
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
				MinimumLogLevel = LogLevel.Error,
				LoggerFactory = seriFactory
			});

			var serviceProvider = new ServiceCollection()
				.AddLogging(loggingBuilderExtensions => loggingBuilderExtensions.AddSerilog(logger))
				.AddSingleton(configProvider)
				.AddSingleton(_client)
				.AddSingleton<UptimeManagementService>()
				.AddSingleton<BeatSaverClientProvider>()
				.AddSingleton<ScoreSaberService>()
				.BuildServiceProvider();


			_client
				.UseCommandsNext(new CommandsNextConfiguration
				{
					EnableMentionPrefix = false,
					EnableDefaultHelp = false,
					CaseSensitive = false,
					DmHelp = false,
					IgnoreExtraArguments = false,
					StringPrefixes = new[] {configProvider.Discord.Prefix},
					Services = serviceProvider
				})
				.RegisterCommands(Assembly.GetEntryAssembly());

			await _client.ConnectAsync().ConfigureAwait(false);

			await Task.Delay(-1).ConfigureAwait(false);
		}
	}
}