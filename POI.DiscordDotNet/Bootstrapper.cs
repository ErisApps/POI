using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using POI.Core.Extensions;
using POI.Core.Services.Interfaces;
using POI.DiscordDotNet.Services;
using POI.DiscordDotNet.Services.Interfaces;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace POI.DiscordDotNet
{
	internal static class Bootstrapper
	{
		private static DiscordClient _client = null!;

		public static async Task Main(string[]? args = null)
		{
			var cultureInfo = new CultureInfo("en-GB");

			CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
			CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

			var dockerized = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
			var pathProvider = new PathProvider(dockerized, args?.Length >= 1 ? args[0] : null);

			var logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.MinimumLevel.Override(nameof(DSharpPlus), LogEventLevel.Information)
				.MinimumLevel.Override(nameof(Microsoft), LogEventLevel.Information)
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
				logger.Fatal("Exiting... Please ensure the config is correct");
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

			var hostBuilder = Host.CreateDefaultBuilder(args)
				.UseSerilog(logger)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					if (!dockerized)
					{
						webBuilder.UseUrls("http://0.0.0.0:5000");
					}

					webBuilder.UseStartup<ApiStartup>();
				})
				.ConfigureServices(sc =>
				{
					sc.AddSingleton<Constants>();
					sc.AddSingleton<IConstantsCore>(provider => provider.GetRequiredService<Constants>());
					sc.AddSingleton<IConstants>(provider => provider.GetRequiredService<Constants>());
					sc.AddCoreServices();
					sc.AddSingleton(configProvider);
					sc.AddSingleton(pathProvider);
					sc.AddSingleton(_client);
					sc.AddSingleton<MongoDbService>();
					sc.AddSingleton<UptimeManagementService>();
				}).Build();

			// Verify mongoDbConnection
			if (!await VerifyMongoDbConnection(hostBuilder.Services, logger))
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
				Services = hostBuilder.Services
			});
			commandsNext.CommandErrored += (_, eventArgs) =>
			{
				logger.Error(eventArgs.Exception, "{Username} tried to execute command {CommandName}, but it errored", eventArgs.Context.User.Username, eventArgs.Command.Name);

				return Task.CompletedTask;
			};
			commandsNext.CommandExecuted += (_, eventArgs) =>
			{
				logger.Debug("{Username} executed command {CommandName}", eventArgs.Context.User.Username, eventArgs.Command.Name);

				return Task.CompletedTask;
			};
			commandsNext.RegisterCommands(Assembly.GetEntryAssembly());

			await _client.ConnectAsync(new DiscordActivity("POI for mod? (pretty please)", ActivityType.Playing)).ConfigureAwait(false);

			await hostBuilder.RunAsync().ConfigureAwait(false);
		}

		private static async Task<bool> VerifyMongoDbConnection(IServiceProvider serviceProvider, Serilog.ILogger logger)
		{
			var mongoDbService = serviceProvider.GetService<MongoDbService>()!;
			if (await mongoDbService.TestConnectivity().ConfigureAwait(false))
			{
				logger.Information("Connected to MongoDb instance");

				return true;
			}

			logger.Fatal("Couldn't connect to database. Exiting...");
			return false;
		}
	}
}