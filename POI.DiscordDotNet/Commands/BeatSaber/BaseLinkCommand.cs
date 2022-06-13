using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using POI.Core.Models.ScoreSaber.Profile;
using POI.Core.Services;
using POI.DiscordDotNet.Commands.Modules.ChatCommands;
using POI.DiscordDotNet.Extensions;
using POI.DiscordDotNet.Services;

namespace POI.DiscordDotNet.Commands.BeatSaber
{
	public abstract class BaseLinkCommand : BeatSaberCommandsModule
	{
		// One day... this will be fetched dynamically from the database... one day...
		private static readonly ulong[] ApproverUserIds =
		{
			261830384663134209 /* Eris */,
			246209289444655105 /* Pyro */,
			212220646732595200 /* Logius */,
			299995209331113985 /* Arno */,
			353308809931784204 /* Jestro */,
			321354941589618698 /* GianniKoch */
		};

		private const string APPROVE_ACTION_ID = "approve";
		private const string DENY_ACTION_ID = "deny";

		private readonly ILogger<BaseLinkCommand> _logger;
		private readonly ScoreSaberApiService _scoreSaberApiService;

		protected readonly UserSettingsService UserSettingsService;

		protected BaseLinkCommand(ILogger<BaseLinkCommand> logger, ScoreSaberApiService scoreSaberApiService, UserSettingsService userSettingsService)
		{
			_logger = logger;
			_scoreSaberApiService = scoreSaberApiService;

			UserSettingsService = userSettingsService;
		}

		public virtual async Task Handle(CommandContext ctx, string _)
		{
			await ctx.TriggerTypingAsync().ConfigureAwait(false);
		}

		protected async Task<BasicProfile?> FetchScoreSaberProfile(CommandContext ctx, string scoreSaberId)
		{
			var playerInfo = await _scoreSaberApiService.FetchBasicPlayerProfile(scoreSaberId);
			if (playerInfo == null)
			{
				await ctx.Message.RespondAsync("I didn't find the scoresaber account. Maybe you made a typo!?!");
			}

			return playerInfo;
		}

		protected static async Task<bool?> WaitForScoreLinkConfirmation(CommandContext ctx, BasicProfile basicProfile, string title)
		{
			var messageBuilder = new DiscordMessageBuilder();
			var embedBuilder = new DiscordEmbedBuilder()
				.WithPoiColor()
				.WithAuthor(ctx.User.Username, iconUrl: ctx.User.GetAvatarUrl(ImageFormat.Auto, 256))
				.WithThumbnail(basicProfile.ProfilePicture)
				.WithTitle(title)
				.WithDescription("ScoreSaberId Regex matching and validation result")
				.AddField("Name", basicProfile.Name, true)
				.AddField("Country", !string.IsNullOrWhiteSpace(basicProfile.Country) ? basicProfile.Country : "N/A", true)
				.AddField("Rank", basicProfile.Rank.ToString(), true)
				.WithFooter($"Request valid for 2 hours (until: {DateTimeOffset.Now.AddHours(2):G}");

			messageBuilder.WithEmbed(embedBuilder.Build());

			var buttons = new[] { new DiscordButtonComponent(ButtonStyle.Success, APPROVE_ACTION_ID, "✅"), new DiscordButtonComponent(ButtonStyle.Danger, DENY_ACTION_ID, "🚫") };
			messageBuilder.AddComponents(buttons.Cast<DiscordComponent>());

			var discordMessage = await ctx.Message.RespondAsync(messageBuilder).ConfigureAwait(false);

			var itv = ctx.Client.GetInteractivity();

			bool hasResponded;
			InteractivityResult<ComponentInteractionCreateEventArgs>? interactivityResult;
			do
			{
				interactivityResult = await itv.WaitForButtonAsync(discordMessage, buttons, TimeSpan.FromHours(2));

				if (interactivityResult.Value.TimedOut)
				{
					return null;
				}

				await interactivityResult.Value.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
				hasResponded = ApproverUserIds.Contains(interactivityResult.Value.Result.User.Id);
			} while (!hasResponded);

			return (interactivityResult.Value.Result.Id == APPROVE_ACTION_ID);
		}

		protected async Task<string?> ExtractScoreSaberId(CommandContext ctx)
		{
			var args = ctx.RawArgumentString
				.Split(" ", StringSplitOptions.RemoveEmptyEntries)
				.Where(arg => !arg.StartsWith("<@!"))
				.Where(arg => !arg.EndsWith(">"))
				.ToList();

			if (args.Count != 1)
			{
				await _logger.LogError(ctx, "No ScoreSaber profile provided", false).ConfigureAwait(false);
				return null;
			}

			if (!args.First().ExtractScoreSaberId(out var scoreSaberId))
			{
				await _logger.LogError(ctx, "Seems like this profile doesn't exist", false).ConfigureAwait(false);
				return null;
			}

			return scoreSaberId;
		}

		protected Task CreateScoreLink(string discordId, string scoreSaberId)
		{
			return UserSettingsService.CreateOrUpdateScoreSaberLink(discordId, scoreSaberId);

			// TODO: Role assignment logic (Preferably call into service)
		}
	}
}