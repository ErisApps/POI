using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
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

			var messageBuilder = await IsProfileValid(ctx);
			if (messageBuilder == null)
			{
				return;
			}

			var buttons = new[]
			{
				new DiscordButtonComponent(ButtonStyle.Success, "approve", "✅"),
				new DiscordButtonComponent(ButtonStyle.Danger, "deny", "🚫")
			};
			messageBuilder.AddComponents(buttons.Cast<DiscordComponent>());

			var discordMessage = await ctx.Message.RespondAsync(messageBuilder).ConfigureAwait(false);

			var itv = ctx.Client.GetInteractivity();

			var hasResponded = false;
			InteractivityResult<ComponentInteractionCreateEventArgs>? task = null;
			while (!hasResponded)
			{
				task = await itv.WaitForButtonAsync(discordMessage, buttons, TimeSpan.FromHours(2));

				if (task.Value.TimedOut)
				{
					await ctx.RespondAsync("Mea culpa, I timed out :c").ConfigureAwait(false);
					break;
				}

				await task.Value.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
				hasResponded = task.Value.Result.User.Id is 148824637004840961 or 261830384663134209;
			}

			if (hasResponded && task != null)
			{
				await ctx.RespondAsync($"{task.Value.Result.User.Username} has responded with actionId: {task.Value.Result.Id}").ConfigureAwait(false);
			}
		}

		protected override DiscordEmbedBuilder EnrichProfileEmbedBuilderShared(DiscordEmbedBuilder embedBuilder)
		{
			embedBuilder
				.WithTitle("ScoreSaberId Regex matching and validation result")
				.WithFooter($"Time remaining: <t:{DateTimeOffset.Now.AddHours(2).ToUnixTimeSeconds()}:R>");

			return embedBuilder;
		}
	}
}