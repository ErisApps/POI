using System.Globalization;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using POI.DiscordDotNet;
using POI.DiscordDotNet.Commands.SlashCommands.Profile;
using POI.DiscordDotNet.Commands.SlashCommands.Test;
using POI.DiscordDotNet.Commands.SlashCommands.Utils;
using POI.DiscordDotNet.Configuration;
using POI.DiscordDotNet.Jobs;
using POI.DiscordDotNet.Services;
using POI.DiscordDotNet.Services.Implementations;
using POI.Persistence.EFCore.Npgsql.Extensions;
using POI.ThirdParty.BeatSaver.Extensions;
using POI.ThirdParty.BeatSavior.Extensions;
using POI.ThirdParty.Core.Services;
using POI.ThirdParty.ScoreSaber.Extensions;
using Quartz;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

Console.WriteLine("Hello from POI, World!");

var builder = WebApplication.CreateBuilder(args);
// Set culture to en-GB to ensure that the decimal separator is always a dot, while keeping the correct date format
var cultureInfo = new CultureInfo("en-GB");
// Set the desired port
builder.WebHost.ConfigureKestrel(options =>
{
	options.ListenAnyIP(5224); // Listen on all available network interfaces
});

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
builder.Services.AddSerilog((configuration) =>
{
	const string logOutputTemplate =
		"[{Timestamp:HH:mm:ss.fff} | {Level:u3} | {SourceContext:l}] {Message:lj}{NewLine}{Exception}";

	var pathConfigurationOptions = Options.Create(builder.Configuration.GetRequiredSection(PathConfigurationOptions.SECTION_NAME).Get<PathConfigurationOptions>()!);
	var pathProvider = new PathProvider(pathConfigurationOptions);

	configuration
		.ReadFrom.Configuration(builder.Configuration)
		.Enrich.FromLogContext()
		.WriteTo.Console(theme: SystemConsoleTheme.Colored, outputTemplate: logOutputTemplate)
		.WriteTo.Async(writeToInternal => writeToInternal.File(
			Path.Combine(pathProvider.LogsPath, "logs.txt"),
			rollingInterval: RollingInterval.Day,
			retainedFileCountLimit: 60,
			buffered: true,
			outputTemplate: logOutputTemplate));
});

// Add constants and third party services
builder.Services
	.AddSingleton<IConstants, Constants>()
	.AddBeatSaver()
	.AddBeatSavior()
	.AddScoreSaber();

// Add persistence layer
var connectionString = builder.Configuration.GetConnectionString("PostgreSQL")!;
builder.Services
	.AddPersistence(connectionString);

// Add path provider
builder.Services.AddOptions<PathConfigurationOptions>()
	.Bind(builder.Configuration.GetSection(PathConfigurationOptions.SECTION_NAME));
builder.Services
	.AddSingleton<PathProvider>();

// Add Discord services
builder.Services.AddOptions<DiscordConfigurationOptions>()
	.Bind(builder.Configuration.GetSection(DiscordConfigurationOptions.SECTION_NAME));

builder.Services
	.AddScoped<IAddDiscordClientFunctionality, DiscordChatCommandsService>()
	.AddScoped<IAddDiscordClientFunctionality, DiscordInteractivityService>()
	.AddScoped<IAddDiscordClientFunctionality, DiscordSlashCommandsService>()
	.AddScoped<IAddDiscordClientFunctionality, DiscordStarboardService>()
	.AddScoped<IAddDiscordClientFunctionality, DiscordPinFirstEventThreadMessageService>()
	.AddScoped<IUptimeManagementService, UptimeManagementService>()
	.AddScoped<IAddDiscordClientFunctionality, UptimeManagementService>(provider =>
		(UptimeManagementService) provider.GetRequiredService<IUptimeManagementService>())
	.AddSingleton<IManageDiscordClientProvider, DiscordClientProvider>()
	.AddSingleton<IDiscordClientProvider, DiscordClientProvider>(provider =>
		(DiscordClientProvider) provider.GetRequiredService<IManageDiscordClientProvider>())
	.AddHostedService<DiscordHostedService>();

builder.Services
	.AddProfileSlashCommands()
	.AddTestSlashCommands()
	.AddUtilitySlashCommands();

// Add and configure Quartz.NET
builder.Services
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

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Map controllers
app.MapControllers();

await app.RunAsync().ConfigureAwait(false);