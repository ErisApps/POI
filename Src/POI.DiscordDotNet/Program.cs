// See https://aka.ms/new-console-template for more information

using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using POI.DiscordDotNet.Configuration;
using POI.DiscordDotNet.Jobs;
using POI.Persistence.EFCore.Npgsql.Extensions;
using POI.DiscordDotNet.Services;
using POI.DiscordDotNet.Services.Implementations;
using POI.ThirdParty.BeatSaver.Extensions;
using POI.ThirdParty.BeatSavior.Extensions;
using POI.ThirdParty.Core.Services;
using POI.ThirdParty.ScoreSaber.Extensions;
using Quartz;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

Console.WriteLine("Hello from POI, World!");

var host = Host.CreateDefaultBuilder()
	.ConfigureAppConfiguration(builder =>
	{
		builder.AddCommandLine(args);

		// Set culture to en-GB to ensure that the decimal separator is always a dot, while keeping the correct date format
		var cultureInfo = new CultureInfo("en-GB");

		CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
		CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
	})
	.UseSerilog((context, configuration) =>
	{
		const string logOutputTemplate =
			"[{Timestamp:HH:mm:ss.fff} | {Level:u3} | {SourceContext:l}] {Message:lj}{NewLine}{Exception}";

		var pathConfigurationOptions = Options.Create(context.Configuration.GetRequiredSection(PathConfigurationOptions.SECTION_NAME).Get<PathConfigurationOptions>()!);
		var pathProvider = new PathProvider(pathConfigurationOptions);

		configuration
			.ReadFrom.Configuration(context.Configuration)
			.Enrich.FromLogContext()
			.WriteTo.Console(theme: SystemConsoleTheme.Colored, outputTemplate: logOutputTemplate)
			.WriteTo.Async(writeToInternal => writeToInternal.File(
				Path.Combine(pathProvider.LogsPath, "logs.txt"),
				rollingInterval: RollingInterval.Day,
				retainedFileCountLimit: 60,
				buffered: true,
				outputTemplate: logOutputTemplate));
	})
	.ConfigureServices((context, services) =>
	{
		// Add constants and third party services
		services
			.AddSingleton<IConstants, Constants>()
			.AddBeatSaver()
			.AddBeatSavior()
			.AddScoreSaber();

		// Add persistence layer
		var connectionString = context.Configuration.GetConnectionString("PostgreSQL")!;
		services
			.AddPersistence(connectionString);

		// Add path provider
		services.AddOptions<PathConfigurationOptions>()
			.Bind(context.Configuration.GetSection(PathConfigurationOptions.SECTION_NAME));
		services
			.AddSingleton<PathProvider>();

		// Add Discord services
		services.AddOptions<DiscordConfigurationOptions>()
			.Bind(context.Configuration.GetSection(DiscordConfigurationOptions.SECTION_NAME));

		services
			.AddScoped<IAddDiscordClientFunctionality, DiscordChatCommandsService>()
			.AddScoped<IAddDiscordClientFunctionality, DiscordInteractivityService>()
			.AddScoped<IAddDiscordClientFunctionality, DiscordSlashCommandsService>()
			.AddScoped<IAddDiscordClientFunctionality, UptimeManagementService>()
			.AddSingleton<IManageDiscordClientProvider, DiscordClientProvider>()
			.AddSingleton<IDiscordClientProvider, DiscordClientProvider>(provider =>
				(DiscordClientProvider) provider.GetRequiredService<IManageDiscordClientProvider>())
			.AddHostedService<DiscordHostedService>();

		// Add and configure Quartz.NET
		services
			.AddQuartz(q =>
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
			})
			.AddQuartzHostedService(options =>
			{
				// delay start of Quartz.NET to ensure that the DiscordHostedService is started first
				options.StartDelay = TimeSpan.FromSeconds(5);
			});
	});

await host.RunConsoleAsync().ConfigureAwait(false);