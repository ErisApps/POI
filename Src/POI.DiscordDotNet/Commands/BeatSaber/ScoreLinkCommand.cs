using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using POI.Persistence.Repositories;
using POI.ThirdParty.ScoreSaber.Services;

namespace POI.DiscordDotNet.Commands.BeatSaber
{
	[UsedImplicitly]
	public class ScoreLinkCommand : BaseLinkCommand
	{
		public ScoreLinkCommand(ILogger<ScoreLinkCommand> logger, IScoreSaberApiService scoreSaberApiService, IGlobalUserSettingsRepository globalUserSettingsRepository,
			IServerDependentUserSettingsRepository serverDependentUserSettingsRepository)
			: base(logger, scoreSaberApiService, globalUserSettingsRepository, serverDependentUserSettingsRepository)
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

			var discordId = ctx.Message.Author.Id;
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

					await ctx.Message.RespondAsync("Yay, congrats. Your scorelink request was approved. ^^").ConfigureAwait(false);

					break;
				case false:
					await ctx.Message.RespondAsync("I'm sorry, your scorelink request was denied :c").ConfigureAwait(false);
					break;
				case null:
					await ctx.Message.RespondAsync("Uh oh... it seems like nobody reviewed your scorelink request in the past 2 hours, please try again when more people are awake ^^").ConfigureAwait(false);
					break;
			}
		}

		private async Task<bool> CheckScoreLinkConflicts(CommandContext ctx, ulong discordId, string scoreSaberId)
		{
			// Check discordId conflict
			var userSettings = await GlobalUserSettingsRepository.LookupSettingsByDiscordId(discordId);
			if (userSettings != null)
			{
				if (userSettings.ScoreSaberId == scoreSaberId)
				{
					await ctx.Message.RespondAsync("Your account is already linked to this ScoreSaber account! O.o").ConfigureAwait(false);
					return true;
				}

				await ctx.Message.RespondAsync($"⚠️Warning: Your account is currently linked to https://scoresaber.com/u/{userSettings.ScoreSaberId} ! Are you sure you want to relink? O.o").ConfigureAwait(false);
			}

			// Check scoreSaberId conflict
			userSettings = await GlobalUserSettingsRepository.LookupSettingsByScoreSaberId(scoreSaberId);
			if (userSettings != null)
			{
				await ctx.Message.RespondAsync($"ScoreSaber account is already linked to <@!{userSettings.UserId}>! O.o").ConfigureAwait(false);
				return true;
			}

			return false;
		}
	}
}