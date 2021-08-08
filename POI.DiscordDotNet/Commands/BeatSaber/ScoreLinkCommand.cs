using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using POI.Core.Services;
using POI.DiscordDotNet.Services;

namespace POI.DiscordDotNet.Commands.BeatSaber
{
	public class ScoreLinkCommand : BaseLinkCommand
	{
		public ScoreLinkCommand(ILogger<ScoreLinkCommand> logger, ScoreSaberApiService scoreSaberApiService, MongoDbService mongoDbService, ScoreSaberLinkService scoreSaberLinkService)
			: base(logger, scoreSaberApiService, mongoDbService, scoreSaberLinkService)
		{
		}

		// ReSharper disable once StringLiteralTypo
		[Command("scorelink")]
		public async Task Handle(CommandContext ctx, [RemainingText] string _)
		{
			var messageBuilder = await IsProfileValid(ctx);

			if (messageBuilder == null)
			{
				return;
			}

			messageBuilder.AddComponents(
				new DiscordButtonComponent(ButtonStyle.Success, "approve", "✅"),
				new DiscordButtonComponent(ButtonStyle.Danger, "deny", "🚫")
			);

			var discordMessage = await ctx.Message.RespondAsync(messageBuilder).ConfigureAwait(false);

			var hasResponded = false;

			while (!hasResponded)
			{
				var waitForButtonAsync = await discordMessage.WaitForButtonAsync().ConfigureAwait(false);

				// await waitForButtonAsync.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

				hasResponded = waitForButtonAsync.Result.User.Id is 148824637004840961 or 261830384663134209;
			}

			await ctx.RespondAsync("has responded").ConfigureAwait(false);
		}

		protected override void Test(CommandContext ctx, DiscordEmbedBuilder embedBuilder)
		{
			// todo: Eris want a better title!
			embedBuilder
				.WithColor(3447003)
				.WithTitle("ScoreSaberId Regex matching and validation result")
				.WithFooter($"Time remaining: <t:{DateTimeOffset.Now.AddHours(2).ToUnixTimeSeconds()}:R>");
		}
	}
}