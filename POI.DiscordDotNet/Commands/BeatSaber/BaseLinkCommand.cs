using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
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

		protected async Task<DiscordMessageBuilder?> IsProfileValid(CommandContext ctx)
		{
			await ctx.TriggerTypingAsync().ConfigureAwait(false);
			string? scoreSaberId;

			// Extract scoreSaberId
			try
			{
				scoreSaberId = ExtractScoreSaberId(ctx);
			}
			catch (Exception e)
			{
				await ctx.Message.RespondAsync(e.Message + "! ><").ConfigureAwait(false);
				return null;
			}

			// Lookup scoreSaberId
			var lookupSoreSaberIdLink = await _scoreSaberLinkService.LookupDiscordId(scoreSaberId);
			if (lookupSoreSaberIdLink != null)
			{
				await ctx.Message.RespondAsync($"ScoreSaber account is already linked to <@!{lookupSoreSaberIdLink}>! O.o").ConfigureAwait(false);
				return null;
			}

			// Lookup discordId
			var lookupDiscordIdLink = await _scoreSaberLinkService.LookupScoreSaberId(ctx.Message.Author.Id.ToString());
			if (lookupDiscordIdLink != null)
			{
				await ctx.Message.RespondAsync($"Your account is already linked to https://scoresaber.com/u/{lookupDiscordIdLink}! O.o").ConfigureAwait(false);
				return null;
			}

			var playerInfo = await _scoreSaberApiService.FetchBasicPlayerProfile(scoreSaberId);
			if (playerInfo == null)
			{
				await ctx.Message.RespondAsync("I didn't find the scoresaber account. Maybe you made a typo!?!");
				return null;
			}

			var messageBuilder = new DiscordMessageBuilder();
			var embedBuilder = new DiscordEmbedBuilder();

			EnrichProfileEmbedBuilderShared(embedBuilder)
				.WithAuthor(ctx.User.Username, iconUrl: ctx.User.GetAvatarUrl(ImageFormat.Auto, 256))
				.WithThumbnail(playerInfo.ProfilePicture)
				.AddField("Name", playerInfo.Name, true)
				.AddField("Country", playerInfo.Country, true)
				.AddField("Rank", playerInfo.Rank.ToString(), true);

			messageBuilder.WithEmbed(embedBuilder.Build());

			return messageBuilder;
		}

		protected static string ExtractScoreSaberId(CommandContext ctx)
		{
			var args = ctx.RawArgumentString
				.Split(" ", StringSplitOptions.RemoveEmptyEntries)
				.Where(arg => !arg.StartsWith("<@!"))
				.Where(arg => !arg.EndsWith(">"))
				.ToList();

			if (args.Count != 1)
			{
				throw new ArgumentException("No scoresaber profile provided");
			}

			if (!args.First().ExtractScoreSaberId(out var scoreSaberId))
			{
				throw new Exception("Seems like this profile doesn't exist");
			}

			return scoreSaberId!;
		}

		protected virtual DiscordEmbedBuilder EnrichProfileEmbedBuilderShared(DiscordEmbedBuilder embedBuilder)
		{
			embedBuilder.WithColor(3447003);

			return embedBuilder;
		}
	}
}