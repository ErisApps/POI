using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using POI.DiscordDotNet.Commands.Modules.ChatCommands;
using POI.DiscordDotNet.Extensions;
using POI.DiscordDotNet.Persistence.Repositories;
using POI.ThirdParty.ScoreSaber.Models.Profile;
using POI.ThirdParty.ScoreSaber.Services;
using Permissions = POI.DiscordDotNet.Persistence.Domain.Permissions;

namespace POI.DiscordDotNet.Commands.BeatSaber
{
	public abstract class BaseLinkCommand : BeatSaberCommandsModule
	{
		private const string APPROVE_ACTION_ID = "approve";
		private const string DENY_ACTION_ID = "deny";

		private readonly ILogger<BaseLinkCommand> _logger;
		private readonly IScoreSaberApiService _scoreSaberApiService;
		private readonly IServerDependentUserSettingsRepository _serverDependentUserSettingsRepository;

		protected readonly IGlobalUserSettingsRepository GlobalUserSettingsRepository;

		protected BaseLinkCommand(ILogger<BaseLinkCommand> logger, IScoreSaberApiService scoreSaberApiService, IGlobalUserSettingsRepository globalUserSettingsRepository,
			IServerDependentUserSettingsRepository serverDependentUserSettingsRepository)
		{
			_logger = logger;
			_scoreSaberApiService = scoreSaberApiService;

			GlobalUserSettingsRepository = globalUserSettingsRepository;
			_serverDependentUserSettingsRepository = serverDependentUserSettingsRepository;
		}

		public virtual async Task Handle(CommandContext ctx, string _)
		{
			await ctx.TriggerTypingAsync().ConfigureAwait(false);
		}

		protected async Task<BasicProfileDto?> FetchScoreSaberProfile(CommandContext ctx, string scoreSaberId)
		{
			var playerInfo = await _scoreSaberApiService.FetchBasicPlayerProfile(scoreSaberId);
			if (playerInfo == null)
			{
				await ctx.Message.RespondAsync("I didn't find the scoresaber account. Maybe you made a typo!?!");
			}

			return playerInfo;
		}

		protected async Task<bool?> WaitForScoreLinkConfirmation(CommandContext ctx, BasicProfileDto basicProfile, string title)
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
				var serverDependentUserSettings = await _serverDependentUserSettingsRepository
					.FindOneById(interactivityResult.Value.Result.User.Id, interactivityResult.Value.Result.Guild.Id)
					.ConfigureAwait(false);
				hasResponded = serverDependentUserSettings != null && serverDependentUserSettings.Permissions.HasFlag(Permissions.LinkApproval);
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

		protected Task CreateScoreLink(ulong discordId, string scoreSaberId)
		{
			return GlobalUserSettingsRepository.CreateOrUpdateScoreSaberLink(discordId, scoreSaberId);

			// TODO: Role assignment logic (Preferably call into service)
		}
	}
}