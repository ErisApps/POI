using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using POI.DiscordDotNet.Commands.SlashCommands.ScoreSaber;

namespace POI.DiscordDotNet.Commands.SlashCommands.Modules;

[SlashCommandGroup("scoresaber", "ScoreSaber related commands"), UsedImplicitly]
public class ScoreSaberSlashCommandsModule : ApplicationCommandModule
{
	[SlashCommand("recent", "Shows your recent ScoreSaber play"), UsedImplicitly]
	public Task HandleRecentCommand(InteractionContext ctx,
		[Option("scoreSaberId", "The ScoreSaber id of the player to show the recent play of.")]
		string? scoreSaberId = null,
		[Option("scoreSaberUser", "The player to show the recent play of.")]
		DiscordUser? discordUser = null,
		[Option("nthSong", "The nth song to show. (Default: 1)")]
		long nthSong = 1)
	{
		return ctx.Services.GetRequiredService<ScoreSaberRecentSongCommand>().Handle(ctx, (int) nthSong, scoreSaberId, discordUser);
	}

	[SlashCommand("top", "Shows your top ScoreSaber play"), UsedImplicitly]
	public Task HandleTopCommand(InteractionContext ctx,
		[Option("scoreSaberId", "The ScoreSaber id of the player to show the top play of.")]
		string? scoreSaberId = null,
		[Option("scoreSaberUser", "The player to show the top play of.")]
		DiscordUser? discordUser = null,
		[Option("nthSong", "The nth song to show. (Default: 1)")]
		long nthSong = 1)
	{
		return ctx.Services.GetRequiredService<ScoreSaberTopSongCommand>().Handle(ctx, (int) nthSong, scoreSaberId, discordUser);
	}
}