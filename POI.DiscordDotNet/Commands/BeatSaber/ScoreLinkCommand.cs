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
				task = await itv.WaitForButtonAsync(discordMessage, buttons, null);

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