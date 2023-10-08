using DSharpPlus.SlashCommands;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using POI.DiscordDotNet.Commands.SlashCommands.Profile;

namespace POI.DiscordDotNet.Commands.SlashCommands.Modules
{
	[SlashCommandGroup("profile", "Personal profile related commands"), UsedImplicitly]
	public class ProfileSlashCommandsModule : ApplicationCommandModule
	{
		[SlashCommandGroup("birthday", "Commands related to managing your birthday"), UsedImplicitly]
		public class BirthdaySlashCommandsModule : ApplicationCommandModule
		{
			[SlashCommand("set", "Sets your birthday"), UsedImplicitly]
			public Task Set(InteractionContext ctx, [Option("date", "The date of your birthday. Format: dd-MM-yyyy Example: 31-10-1998")] string birthdayDateRaw)
				=> ctx.Services.GetRequiredService<BirthdaySlashCommands>().Set(ctx, birthdayDateRaw);

			[SlashCommand("clear", "Unsets your birthday"), UsedImplicitly]
			public Task Clear(InteractionContext ctx)
				=> ctx.Services.GetRequiredService<BirthdaySlashCommands>().Clear(ctx);
		}

		[SlashCommandGroup("link", "Link community Beat Saber services to your profile."), UsedImplicitly]
		public class LinkSlashCommandsModule : ApplicationCommandModule
		{
			[SlashCommand("scoresaber", "ScoreSaber is a score tracking site for Beat Saber."), UsedImplicitly]
			public Task ScoreSaber(InteractionContext ctx)
				=> ctx.Services.GetRequiredService<LinkSlashCommands>().ScoreSaber(ctx);

			[SlashCommand("beatleader", "BeatLeader is a score tracking site for Beat Saber."), UsedImplicitly]
			public Task BeatLeader(InteractionContext ctx)
				=> ctx.Services.GetRequiredService<LinkSlashCommands>().BeatLeader(ctx);

			[SlashCommand("beatsaver", "BeatSaver is a beat map distribution site."), UsedImplicitly]
			public Task BeatSaver(InteractionContext ctx)
				=> ctx.Services.GetRequiredService<LinkSlashCommands>().BeatSaver(ctx);
		}
	}
}