using Microsoft.EntityFrameworkCore;
using POI.Persistence.Domain;

namespace POI.Persistence.EFCore.Npgsql.Infrastructure;

internal class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}

	public DbSet<ServerSettings> ServerSettings { get; set; }

	public DbSet<GlobalUserSettings> GlobalUserSettings { get; set; }
	public DbSet<ServerDependentUserSettings> ServerDependentUserSettings { get; set; }

	public DbSet<LeaderboardEntry> LeaderboardEntries { get; set; }

	public DbSet<StarboardMessages> StarboardMessages { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		var serverSettingsModelBuilder = modelBuilder.Entity<ServerSettings>();
		serverSettingsModelBuilder.HasKey(x => x.ServerId);

		var globalUserSettingsModelBuilder = modelBuilder.Entity<GlobalUserSettings>();
		globalUserSettingsModelBuilder.HasKey(x => x.DiscordUserId);
		globalUserSettingsModelBuilder.Property(x => x.ScoreSaberId).IsRequired(false).HasMaxLength(20);
		globalUserSettingsModelBuilder.HasIndex(x => x.ScoreSaberId).IsUnique();;

		var serverDependentUserSettingsModelBuilder = modelBuilder.Entity<ServerDependentUserSettings>();
		serverDependentUserSettingsModelBuilder.HasKey(x => new { UserId = x.DiscordUserId, x.ServerId});

		var leaderboardEntryModelBuilder = modelBuilder.Entity<LeaderboardEntry>();
		leaderboardEntryModelBuilder.HasKey(x => x.ScoreSaberId);
		leaderboardEntryModelBuilder.Property(x => x.ScoreSaberId).HasMaxLength(20);
		leaderboardEntryModelBuilder.Property(x => x.CountryRank).IsRequired();
		leaderboardEntryModelBuilder.Property(x => x.Name).IsRequired();
		leaderboardEntryModelBuilder.Property(x => x.Pp).IsRequired();

		var starboardMessagesModelBuilder = modelBuilder.Entity<StarboardMessages>();
		starboardMessagesModelBuilder.HasKey(x => new { x.ServerId, x.ChannelId, x.MessageId });
	}
}