using System.Globalization;
using System.Reflection;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using POI.Core.Services;
using POI.DiscordDotNet.Jobs;
using POI.DiscordDotNet.Persistence.EFCore.Npgsql.Extensions;
using POI.DiscordDotNet.Services;
using POI.DiscordDotNet.Services.Implementations;
using POI.ThirdParty.BeatSaver.Extensions;
using POI.ThirdParty.BeatSavior.Extensions;
using POI.ThirdParty.ScoreSaber.Extensions;
using Quartz;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace POI.DiscordDotNet
{
	internal static class Bootstrapper
	{
		private static DiscordClient _client = null!;

		private const string LOG_OUTPUT_TEMPLATE = "[{Timestamp:HH:mm:ss.fff} {Level:u3} {SourceContext:l}] {Message:lj}{NewLine}{Exception}";

		public static async Task Main(string[]? args = null)
		{
			var cultureInfo = new CultureInfo("en-GB");

			CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
			CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

			var dockerized = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
			var pathProvider = new PathProvider(null);

			var logger = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.WriteTo.Console(theme: SystemConsoleTheme.Colored, outputTemplate: LOG_OUTPUT_TEMPLATE)
				.WriteTo.Conditional(_ => dockerized,
					(writeTo => writeTo.Async(
						writeToInternal => writeToInternal.File(pathProvider.LogsPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 60, buffered: true, outputTemplate: LOG_OUTPUT_TEMPLATE)
					)))
				.CreateLogger();

			var configProvider = new ConfigProviderService(logger.ForContext<ConfigProviderService>(), null);
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
				Intents = DiscordIntents.DirectMessages | DiscordIntents.Guilds | DiscordIntents.GuildMessages | DiscordIntents.GuildVoiceStates,
				// This is apparently a bad idea according to the documentation... but I'm going to enable it regardless...
				ReconnectIndefinitely = true
			});

			var hostBuilder = Host.CreateDefaultBuilder(args)
				.UseSerilog(logger)
				.ConfigureServices(sc =>
				{
					sc.AddSingleton<Constants>()
						.AddSingleton<IConstantsCore>(provider => provider.GetRequiredService<Constants>())
						.AddSingleton<IConstants>(provider => provider.GetRequiredService<Constants>())
						.AddBeatSaver()
						.AddBeatSavior()
						.AddScoreSaber()
						// This is actually a PostgreSQL connectino string
						// TODO: Refactor naming, maybe just switch over to Microsoft.Extensions.* stuff
						.AddPersistence(configProvider.MongoDb.MongoDbConnectionString!)
						.AddSingleton(configProvider)
						.AddSingleton(pathProvider)
						.AddSingleton(_client)
						.AddSingleton<UptimeManagementService>()
						.AddSingleton<DiscordSlashCommandsService>();

					sc.AddSingleton<RankUpFeedJob>();

					sc.AddQuartz(q =>
					{
						// handy when part of cluster or you want to otherwise identify multiple schedulers
						q.SchedulerId = "Scheduler-Core";

						// as of 3.3.2 this also injects scoped services (like EF DbContext) without problems
						q.UseMicrosoftDependencyInjectionJobFactory();

						// these are the defaults
						q.UseSimpleTypeLoader();
						q.UseInMemoryStore();
						q.UseDefaultThreadPool(tp =>
						{
							tp.MaxConcurrency = 5;
						});

						q.ScheduleJob<RankUpFeedJob>(trigger => trigger
							.WithIdentity("RankUpFeed Trigger")
							.WithSchedule(CronScheduleBuilder.CronSchedule("0 0/5 * * * ?")));
						q.ScheduleJob<BirthdayGirlsJob>(trigger => trigger
							.WithIdentity("Birthday Girl trigger")
							.WithCronSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0)));
					});
				}).Build();

			SetupCommandsNext(hostBuilder.Services, logger, configProvider);

			_client.UseInteractivity();

			var slashCommandsManagementService = hostBuilder.Services.GetRequiredService<DiscordSlashCommandsService>()!;
			// slashCommandsManagementService.Setup();

			_ = hostBuilder.Services.GetRequiredService<UptimeManagementService>();

			await _client.ConnectAsync(new DiscordActivity("POI for mod? (pretty please)", ActivityType.Playing)).ConfigureAwait(false);

			var scheduler = await hostBuilder.Services.GetRequiredService<ISchedulerFactory>().GetScheduler();
			await scheduler.Start();

			await hostBuilder.RunAsync().ConfigureAwait(false);
		}

		// TODO: Deprecate in due time...
		private static void SetupCommandsNext(IServiceProvider services, Serilog.ILogger logger, ConfigProviderService configProvider)
		{
			var commandsNext = _client.UseCommandsNext(new CommandsNextConfiguration
			{
				EnableMentionPrefix = false,
				EnableDefaultHelp = false,
				CaseSensitive = false,
				DmHelp = false,
				IgnoreExtraArguments = false,
				StringPrefixes = new[] { configProvider.Discord.Prefix },
				Services = services
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
		}
	}
}