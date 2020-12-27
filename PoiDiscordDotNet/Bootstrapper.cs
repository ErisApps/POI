using System;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PoiDiscordDotNet.Services;
using Serilog;
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
			var token = Environment.GetEnvironmentVariable("token");
			if (string.IsNullOrWhiteSpace(token))
			{
				return;
			}

			Log.Logger = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.WriteTo.Console(theme: AnsiConsoleTheme.Literate)
				.CreateLogger();
			var seriFactory = new LoggerFactory().AddSerilog();

			_client = new DiscordClient(new DiscordConfiguration
			{
				Token = token,
				TokenType = TokenType.Bot,
				MinimumLogLevel = LogLevel.Trace,
				LoggerFactory = seriFactory
			});

			var serviceProvider = new ServiceCollection()
				.AddLogging(loggingBuilderExtensions => loggingBuilderExtensions.AddSerilog())
				.AddSingleton(_client)
				.AddSingleton<UptimeManagementService>()
				.AddSingleton<BeatSaverClientProvider>()
				.AddSingleton<ScoreSaberService>()
				.BuildServiceProvider();

			serviceProvider.GetService<ScoreSaberService>();

			_client
				.UseCommandsNext(new CommandsNextConfiguration
				{
					EnableMentionPrefix = false,
					EnableDefaultHelp = false,
					CaseSensitive = false,
					DmHelp = false,
					IgnoreExtraArguments = false,
					StringPrefixes = new[] {"poinext "},
					Services = serviceProvider
				})
				.RegisterCommands(Assembly.GetEntryAssembly());

			await _client.ConnectAsync().ConfigureAwait(false);

			await Task.Delay(-1).ConfigureAwait(false);
		}
	}
}