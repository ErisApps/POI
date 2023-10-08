using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using JetBrains.Annotations;
using POI.Persistence.Repositories;

namespace POI.DiscordDotNet.Commands.SlashCommands.Profile;

[UsedImplicitly]
public class LinkSlashCommands
{
	private readonly ILinkRequestTokensRepository _linkRequestTokenRepository;

	public LinkSlashCommands(ILinkRequestTokensRepository linkRequestTokenRepository)
	{
		_linkRequestTokenRepository = linkRequestTokenRepository;
	}

	[SlashCommand("scoresaber", "ScoreSaber is a score tracking site for Beat Saber."), UsedImplicitly]
	public async Task ScoreSaber(InteractionContext ctx)
	{
		var token = await _linkRequestTokenRepository.CreateToken(ctx.User.Id);
		var redirectUrl = $"http://localhost:5224/link/scoresaber?loginToken={token}";

		var embed = new DiscordEmbedBuilder
		{
			Color = DiscordColor.Gold,
			Title = "Lets link your ScoreSaber account!",
			Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = "https://scoresaber.com/512x512.png" },
			Description = $"ScoreSaber is a third party leaderboard service.\nFamous for its focus on accuracy and tech maps.\n\n[Connect your ScoreSaber account here]({redirectUrl})"
		};

		await ctx.CreateResponseAsync(embed, true).ConfigureAwait(false);
	}

	[SlashCommand("beatleader", "BeatLeader is a score tracking site for Beat Saber."), UsedImplicitly]
	public async Task BeatLeader(InteractionContext ctx)
	{
		var token = await _linkRequestTokenRepository.CreateToken(ctx.User.Id);
		var redirectUrl = $"http://localhost:5224/link/beatleader?loginToken={token}";

		var embed = new DiscordEmbedBuilder
		{
			Color = DiscordColor.Purple,
			Title = "Lets link your BeatLeader account!",
			Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = "https://files.mastodon.social/accounts/avatars/110/742/419/818/606/340/original/19fb3982c99ec6fc.png" },
			Description = $"BeatLeader is a third party leaderboard service.\nFamous for its focus on speed maps.\n\n[Connect your BeatLeader account here]({redirectUrl})"
		};

		await ctx.CreateResponseAsync(embed, true).ConfigureAwait(false);
	}

	[SlashCommand("beatsaver", "BeatSaver is a beat map distribution site."), UsedImplicitly]
	public async Task BeatSaver(InteractionContext ctx)
	{
		var token = await _linkRequestTokenRepository.CreateToken(ctx.User.Id);
		var redirectUrl = $"http://localhost:5224/link/beatsaver?loginToken={token}";

		var embed = new DiscordEmbedBuilder
		{
			Color = DiscordColor.HotPink,
			Title = "Lets link your BeatSaver account!",
			Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = "https://beatsaver.com/static/favicon/apple-touch-icon.png" },
			Description = $"BeatSaver is a third party beat map distribution service.\n\n[Connect your BeatLeader account here]({redirectUrl})"
		};

		await ctx.CreateResponseAsync(embed, true).ConfigureAwait(false);
	}
}