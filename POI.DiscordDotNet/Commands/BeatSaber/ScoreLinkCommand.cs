using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using POI.Core.Services;
using POI.DiscordDotNet.Services;

namespace POI.DiscordDotNet.Commands.BeatSaber
{
	public class ScoreLinkCommand : BaseLinkCommand
	{
		public ScoreLinkCommand(ILogger<ScoreLinkCommand> logger, ScoreSaberApiService scoreSaberApiService, ScoreSaberLinkService scoreSaberLinkService)
			: base(logger, scoreSaberApiService, scoreSaberLinkService)
		{
		}

		// ReSharper disable once StringLiteralTypo
		[Command("scorelink")]
		public override async Task Handle(CommandContext ctx, [RemainingText] string _)
		{
			await base.Handle(ctx, _).ConfigureAwait(false);

			var scoreSaberId = await ExtractScoreSaberId(ctx).ConfigureAwait(false);
			if (scoreSaberId == null)
			{
				return;
			}

			var discordId = ctx.Message.Author.Id.ToString();
			if (await CheckScoreLinkConflicts(ctx, discordId, scoreSaberId).ConfigureAwait(false))
			{
				return;
			}

			var playerProfile = await FetchScoreSaberProfile(ctx, scoreSaberId).ConfigureAwait(false);
			if (playerProfile == null)
			{
				return;
			}

			var scoreLinkApproval = await WaitForScoreLinkConfirmation(ctx, playerProfile, "ScoreLink request confirmation?").ConfigureAwait(false);
			switch (scoreLinkApproval)
			{
				case true:
					await CreateScoreLink(discordId, scoreSaberId).ConfigureAwait(false);
					// TODO: Role assignment logic

					break;
				case false:
					await ctx.Message.RespondAsync("I'm sorry, your scorelink request was denied :c").ConfigureAwait(false);
					break;
				case null:
					await ctx.Message.RespondAsync("Uh oh... it seems like nobody reviewed your scorelink request in the past 2 hours, please try again when more people are awake ^^").ConfigureAwait(false);
					break;
			}
		}
	}
}