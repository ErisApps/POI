﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using POI.DiscordDotNet.Commands.ChatCommands.BeatSaber;
using POI.DiscordDotNet.Commands.ChatCommands.Helpers;
using POI.Persistence.Domain;
using POI.Persistence.Repositories;
using POI.ThirdParty.ScoreSaber.Services;

namespace POI.DiscordDotNet.Commands.ChatCommands.Admin
{
	[UsedImplicitly]
	[RequiresUserSettingsPermission(Permissions.ForceLink)]
	public class ForceLinkCommand : BaseLinkCommand
	{
		public ForceLinkCommand(ILogger<ForceLinkCommand> logger, IScoreSaberApiService scoreSaberApiService, IGlobalUserSettingsRepository globalUserSettingsRepository,
			IServerDependentUserSettingsRepository serverDependentUserSettingsRepository)
			: base(logger, scoreSaberApiService, globalUserSettingsRepository, serverDependentUserSettingsRepository)
		{
		}

		// ReSharper disable once StringLiteralTypo
		[Command("forcelink")]
		public override async Task Handle(CommandContext ctx, [RemainingText] string _)
		{
			await base.Handle(ctx, _).ConfigureAwait(false);

			DiscordUser user;
			ulong discordId;
			switch (ctx.Message.MentionedUsers.Count)
			{
				case 0:
					await ctx.Message.RespondAsync("Target member wasn't mentioned").ConfigureAwait(false);
					return;
				case >= 2:
					await ctx.Message.RespondAsync("More than 1 target member was mentioned").ConfigureAwait(false);
					return;
				default:
					user = ctx.Message.MentionedUsers[0];
					discordId = user.Id;
					break;
			}

			var scoreSaberId = await ExtractScoreSaberId(ctx).ConfigureAwait(false);
			if (scoreSaberId == null)
			{
				return;
			}

			// TODO: Check for possible conflicts

			var playerProfile = await FetchScoreSaberProfile(ctx, scoreSaberId).ConfigureAwait(false);
			if (playerProfile == null)
			{
				return;
			}

			var forceLinkApproval = await WaitForScoreLinkConfirmation(ctx, playerProfile, "ForceLink request confirmation?").ConfigureAwait(false);
			switch (forceLinkApproval)
			{
				case true:
					await CreateScoreLink(discordId, scoreSaberId).ConfigureAwait(false);

					await ctx.Message.RespondAsync($"Yay, congrats <@{discordId}>. Your forcelink request was approved. ^^").ConfigureAwait(false);

					break;
				case false:
					await ctx.Message.RespondAsync($"I'm sorry, the forcelink request for user {user.Username} was denied :c").ConfigureAwait(false);
					break;
				case null:
					await ctx.Message.RespondAsync("Uh oh... it seems like nobody reviewed the forcelink request in the past 2 hours, please try again when more people are awake ^^")
						.ConfigureAwait(false);
					break;
			}
		}
	}
}