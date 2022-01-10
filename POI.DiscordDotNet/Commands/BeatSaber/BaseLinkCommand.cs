using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using POI.Core.Models.ScoreSaber.Profile;
using POI.Core.Services;
using POI.DiscordDotNet.Commands.Modules;
using POI.DiscordDotNet.Extensions;
using POI.DiscordDotNet.Services;

namespace POI.DiscordDotNet.Commands.BeatSaber
{
	public abstract class BaseLinkCommand : BeatSaberCommandsModule
	{
		private readonly ILogger<BaseLinkCommand> _logger;
		private readonly ScoreSaberLinkService _scoreSaberLinkService;
		private readonly ScoreSaberApiService _scoreSaberApiService;

		protected BaseLinkCommand(ILogger<BaseLinkCommand> logger, ScoreSaberApiService scoreSaberApiService, ScoreSaberLinkService scoreSaberLinkService)
		{
			_logger = logger;
			_scoreSaberApiService = scoreSaberApiService;
			_scoreSaberLinkService = scoreSaberLinkService;
		}

		public virtual async Task Handle(CommandContext ctx, string _)
		{
			await ctx.TriggerTypingAsync().ConfigureAwait(false);
		}

		protected async Task<bool> CheckScoreLinkConflicts(CommandContext ctx, string scoreSaberId)
		{
			// Lookup scoreSaberId
			var lookupSoreSaberIdLink = await _scoreSaberLinkService.LookupDiscordId(scoreSaberId);
			if (lookupSoreSaberIdLink != null)
			{
				await ctx.Message.RespondAsync($"ScoreSaber account is already linked to <@!{lookupSoreSaberIdLink}>! O.o").ConfigureAwait(false);
				return true;
			}

			// Lookup discordId
			var lookupDiscordIdLink = await _scoreSaberLinkService.LookupScoreSaberId(ctx.Message.Author.Id.ToString());
			if (lookupDiscordIdLink != null)
			{
				await ctx.Message.RespondAsync($"Your account is already linked to https://scoresaber.com/u/{lookupDiscordIdLink}! O.o").ConfigureAwait(false);
				return true;
			}

			return false;
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

		protected static DiscordMessageBuilder CreateScoreSaberProfileEmbed(CommandContext ctx, BasicProfile basicProfile)
		{
			var messageBuilder = new DiscordMessageBuilder();
			var embedBuilder = new DiscordEmbedBuilder()
				.WithPoiColor()
				.WithAuthor(ctx.User.Username, iconUrl: ctx.User.GetAvatarUrl(ImageFormat.Auto, 256))
				.WithThumbnail(basicProfile.ProfilePicture)
				.WithTitle("ScoreSaberId Regex matching and validation result")
				.AddField("Name", basicProfile.Name, true)
				.AddField("Country", basicProfile.Country, true)
				.AddField("Rank", basicProfile.Rank.ToString(), true);

			messageBuilder.WithEmbed(embedBuilder.Build());

			return messageBuilder;
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
			}

			if (!args.First().ExtractScoreSaberId(out var scoreSaberId))
			{
				await _logger.LogError(ctx, "Seems like this profile doesn't exist", false).ConfigureAwait(false);
			}

			return scoreSaberId;
		}

		protected Task CreateScoreLink(string discordId, string scoreSaberId)
		{
			return _scoreSaberLinkService.CreateScoreSaberLink(discordId, scoreSaberId);
		}
	}
}