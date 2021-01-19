using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using ImageMagick;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PoiDiscordDotNet.Commands.Modules;
using PoiDiscordDotNet.Extensions;
using PoiDiscordDotNet.Models.Database;
using PoiDiscordDotNet.Models.ScoreSaber.Scores;
using PoiDiscordDotNet.Services;

namespace PoiDiscordDotNet.Commands.Beat_Saber
{
	public abstract class BaseSongCommand : BeatSaberCommandsModule
	{
		private readonly ILogger<BaseSongCommand> _logger;
		private readonly DiscordClient _client;
		private readonly string _backgroundImagePath;
		private readonly MongoDbService _mongoDbService;

		protected readonly ScoreSaberService ScoreSaberService;

		protected BaseSongCommand(ILogger<BaseSongCommand> logger, DiscordClient client, ScoreSaberService scoreSaberService, MongoDbService mongoDbService, string backgroundImagePath)
		{
			_logger = logger;
			_client = client;

			ScoreSaberService = scoreSaberService;
			_mongoDbService = mongoDbService;
			_backgroundImagePath = backgroundImagePath;
		}

		protected abstract Task<ScoresPage?> FetchScorePage(string playerId, int page);

		protected async Task GenerateScoreImageAndSendInternal(CommandContext ctx)
		{
			await ctx.TriggerTypingAsync().ConfigureAwait(false);

			var arguments = await ExtractArguments(ctx).ConfigureAwait(false);
			if (arguments == null)
			{
				await ctx.Errored("Something went wrong while trying to parse the arguments").ConfigureAwait(false);
				return;
			}

			(string scoreSaberId, var nthSong) = arguments.Value;

			var profile = await ScoreSaberService.FetchBasicPlayerProfile(scoreSaberId).ConfigureAwait(false);
			if (profile == null)
			{
				await ctx.Errored("Couldn't fetch profile.").ConfigureAwait(false);
				return;
			}

			var songPageNumber = (int) Math.Ceiling((double) nthSong / ScoreSaberService.PLAYS_PER_PAGE);
			var songPage = await FetchScorePage(profile.PlayerInfo.PlayerId, songPageNumber).ConfigureAwait(false);
			if (songPage == null)
			{
				await ctx.Errored("Couldn't fetch songPage.").ConfigureAwait(false);
				return;
			}

			SongScore requestedSong;
			var localSongIndex = --nthSong % ScoreSaberService.PLAYS_PER_PAGE;
			if (songPage.Scores.Count > localSongIndex)
			{
				requestedSong = songPage.Scores[localSongIndex];
			}
			else
			{
				await ctx.Errored("The user doesn't have this many scores just yet.");
				return;
			}

			var coverImageBytes = await ScoreSaberService.FetchCoverImageByHash(requestedSong.SongHash).ConfigureAwait(false);

			await using var memoryStream = new MemoryStream();
			using (var background = new MagickImage(_backgroundImagePath))
			{
				// Cover image
				if (coverImageBytes != null)
				{
					using var coverImage = new MagickImage(coverImageBytes);
					coverImage.Resize(300, 300);
					background.Composite(coverImage, 50, 50, CompositeOperator.Over);
				}

				// Song title
				var titleCaptionSettings = new MagickReadSettings
				{
					Height = 100, Width = 600,
					//BackgroundColor = MagickColors.Fuchsia
				};
				using (var titleCaption = new MagickImage($"caption:{requestedSong.SongName}", titleCaptionSettings))
				{
					background.Composite(titleCaption, 400, 50, CompositeOperator.Over);
				}

				// Artist(s) / Author
				var authorCaptionSettings = new MagickReadSettings
				{
					Height = 100, Width = 600,
					//BackgroundColor = MagickColors.Pink
				};
				using (var authorCaption = new MagickImage($"caption:{requestedSong.SongAuthorName}", authorCaptionSettings))
				{
					background.Composite(authorCaption, 400, 150, CompositeOperator.Over);
				}

				// Difficulty
				var difficultyCaptionSettings = new MagickReadSettings
				{
					Height = 100,
					Width = 600,
					FillColor = MagickColors.HotPink
					//BackgroundColor = MagickColors.Aquamarine
				};
				using (var difficultyCaption = new MagickImage($"caption:{requestedSong.DifficultyRaw}", difficultyCaptionSettings))
				{
					background.Composite(difficultyCaption, 400, 250, CompositeOperator.Over);
				}

				// Mapper
				var mapperCaptionSettings = new MagickReadSettings
				{
					Height = 100,
					Width = 300,
					TextGravity = Gravity.Center
					//BackgroundColor = MagickColors.Aquamarine
				};
				using (var mapperCaption = new MagickImage($"caption:{requestedSong.LevelAuthorName}", mapperCaptionSettings))
				{
					background.Composite(mapperCaption, 50, 375, CompositeOperator.Over);
				}

				// Accuracy (currently only works when provided by ScoreSaber, BeatSaver fallback not implemented yet)
				if (requestedSong.MaxScore > 0)
				{
					var accuracyCaptionSettings = new MagickReadSettings
					{
						Height = 100,
						Width = 150,
						TextGravity = Gravity.Center,
						FontPointsize = 30
						//BackgroundColor = MagickColors.Aquamarine
					};
					using var accuracyCaption = new MagickImage($"label:Accuracy\n{((float) (requestedSong.UnmodifiedScore * 100) / requestedSong.MaxScore):F2}%", accuracyCaptionSettings);
					background.Composite(accuracyCaption, 400, 375, CompositeOperator.Over);
				}

				// Rank
				var rankCaptionSettings = new MagickReadSettings
				{
					Height = 100,
					Width = 150,
					TextGravity = Gravity.Center,
					FontPointsize = 30
					//BackgroundColor = MagickColors.Aquamarine
				};
				using (var rankCaption = new MagickImage($"label:Rank\n{requestedSong.Rank}", rankCaptionSettings))
				{
					background.Composite(rankCaption, 600, 375, CompositeOperator.Over);
				}

				if (requestedSong.Weight > 0)
				{
					// Raw PP
					var rawPpCaptionSettings = new MagickReadSettings
					{
						Height = 100,
						Width = 300,
						TextGravity = Gravity.Center,
						FontPointsize = 40
						//BackgroundColor = MagickColors.Aquamarine
					};
					using (var rawPpCaption = new MagickImage($"label:Raw PP\n{requestedSong.Pp:F3}", rawPpCaptionSettings))
					{
						background.Composite(rawPpCaption, 50, 500, CompositeOperator.Over);
					}

					// Weighted PP
					var weightedPpCaptionSettings = new MagickReadSettings
					{
						Height = 100,
						Width = 300,
						TextGravity = Gravity.Center,
						FontPointsize = 40
						//BackgroundColor = MagickColors.Aquamarine
					};
					using var weightedPpCaption = new MagickImage($"label:Weighted PP\n{(requestedSong.Pp * requestedSong.Weight):F3}", weightedPpCaptionSettings);
					background.Composite(weightedPpCaption, 400, 500, CompositeOperator.Over);
				}

				// Rank
				var timeSetCaptionSettings = new MagickReadSettings
				{
					Height = 100,
					Width = 300,
					TextGravity = Gravity.South,
					FontPointsize = 20
					//BackgroundColor = MagickColors.Aquamarine
				};
				using (var timeSetCaption = new MagickImage($"label:{requestedSong.TimeSet.ToString()}", timeSetCaptionSettings))
				{
					background.Composite(timeSetCaption, 700, 500, CompositeOperator.Over);
				}

				await background.WriteAsync(memoryStream).ConfigureAwait(false);
				await memoryStream.FlushAsync().ConfigureAwait(false);
				memoryStream.Seek(0, SeekOrigin.Begin);
			}

			var messageBuilder = new DiscordMessageBuilder()
				.WithContent("Just a proof of concept thingy, please ignore this.")
				.WithFile("test.jpeg", memoryStream);
			await ctx.Message
				.RespondAsync(messageBuilder)
				.ConfigureAwait(false);
		}

		private async Task<(string, int)?> ExtractArguments(CommandContext ctx)
		{
			string? scoreSaberId = null;

			async Task LookupScoreSaberLink(string discordId)
			{
				try
				{
					var userScoreLinks = await _mongoDbService
						.GetCollection<ScoreSaberLink>()
						.FindAsync(new ExpressionFilterDefinition<ScoreSaberLink>(link => link.DiscordId == discordId))
						.ConfigureAwait(false);
					scoreSaberId = userScoreLinks.FirstOrDefault()?.ScoreSaberId;
				}
				catch (Exception)
				{
					_logger.LogWarning("Couldn't find scoreLink for user {Username}", ctx.Message.Author.Username);
					scoreSaberId = null;
				}
			}

			var args = ctx.RawArguments
				.Where(arg => !string.IsNullOrWhiteSpace(arg))
				.Where(arg => !arg.StartsWith("<@!"))
				.Where(arg => !arg.EndsWith(">"))
				.ToList();

			if (args.Any() && args[0].ExtractScoreSaberId(out scoreSaberId))
			{
				args.RemoveAt(0);
			}
			else if (ctx.Message.MentionedUsers.Any())
			{
				var mentionedUser = ctx.Message.MentionedUsers.First();
				await LookupScoreSaberLink(mentionedUser.Id.ToString()).ConfigureAwait(false);
			}
			else
			{
				await LookupScoreSaberLink(ctx.User.Id.ToString()).ConfigureAwait(false);
			}

			if (scoreSaberId == null)
			{
				return null;
			}

			if (!args.Any() || !int.TryParse(args[0], out var nthSong))
			{
				nthSong = 1;
			}

			return (scoreSaberId, nthSong);
		}
	}
}