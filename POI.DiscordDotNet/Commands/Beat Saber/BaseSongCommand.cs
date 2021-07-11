using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BeatSaverSharp.Models;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using ImageMagick;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NodaTime;
using POI.Core.Models.ScoreSaber.Scores;
using POI.Core.Services;
using POI.DiscordDotNet.Commands.Modules;
using POI.DiscordDotNet.Models.Database;
using POI.DiscordDotNet.Services;
using POI.DiscordDotNet.Extensions;

namespace POI.DiscordDotNet.Commands.Beat_Saber
{
	public abstract class BaseSongCommand : BeatSaberCommandsModule
	{
		private readonly ILogger<BaseSongCommand> _logger;
		private readonly DiscordClient _client;
		private readonly string _backgroundImagePath;
		private readonly string _erisSignaturePath;
		private readonly MongoDbService _mongoDbService;
		private readonly BeatSaverClientProvider _beatSaverClientProvider;

		protected readonly ScoreSaberApiService ScoreSaberApiService;

		protected BaseSongCommand(ILogger<BaseSongCommand> logger, DiscordClient client, ScoreSaberApiService scoreSaberApiService, MongoDbService mongoDbService,
			BeatSaverClientProvider beatSaverClientProvider, string backgroundImagePath, string erisSignaturePath)
		{
			_logger = logger;
			_client = client;

			ScoreSaberApiService = scoreSaberApiService;
			_mongoDbService = mongoDbService;
			_beatSaverClientProvider = beatSaverClientProvider;
			_backgroundImagePath = backgroundImagePath;
			_erisSignaturePath = erisSignaturePath;
		}

		protected abstract Task<ScoresPage?> FetchScorePage(string playerId, int page);

		protected async Task GenerateScoreImageAndSendInternal(CommandContext ctx)
		{
			await ctx.TriggerTypingAsync().ConfigureAwait(false);

			var arguments = await ExtractArguments(ctx).ConfigureAwait(false);
			if (arguments == null)
			{
				await _logger.LogError(ctx, "Something went wrong while trying to parse the arguments").ConfigureAwait(false);
				return;
			}

			var (scoreSaberId, nthSong) = arguments.Value;

			var profile = await ScoreSaberApiService.FetchBasicPlayerProfile(scoreSaberId).ConfigureAwait(false);
			if (profile == null)
			{
				await _logger.LogError(ctx, "Couldn't fetch profile").ConfigureAwait(false);
				return;
			}

			var songPageNumber = (int) Math.Ceiling((double) nthSong / ScoreSaberApiService.PLAYS_PER_PAGE);
			var songPage = await FetchScorePage(profile.PlayerInfo.PlayerId, songPageNumber).ConfigureAwait(false);
			if (songPage == null)
			{
				await _logger.LogError(ctx, "Couldn't fetch songPage").ConfigureAwait(false);
				return;
			}

			SongScore requestedSong;
			var localSongIndex = --nthSong % ScoreSaberApiService.PLAYS_PER_PAGE;
			if (songPage.Scores.Count > localSongIndex)
			{
				requestedSong = songPage.Scores[localSongIndex];
			}
			else
			{
				await _logger.LogError(ctx, "The user doesn't have this many scores just yet").ConfigureAwait(false);
				return;
			}

			if (!requestedSong.DifficultyRaw.ParseScoreSaberDifficulty(out var characteristic, out var difficulty))
			{
				await _logger.LogError(ctx, "Failed to parse ScoreSaber difficulty").ConfigureAwait(false);
				return;
			}

			var maxScore = requestedSong.MaxScore;
			if (maxScore <= 0)
			{
				var beatmap = await _beatSaverClientProvider.GetClientInstance().BeatmapByHash(requestedSong.SongHash, skipCacheCheck: true).ConfigureAwait(false);

				var mappedCharacteristic = BeatmapExtensions.MapToBeatmapCharacteristic(characteristic!);
				var mappedDifficulty = BeatmapExtensions.MapToBeatSaverBeatmapDifficulty(difficulty!);

				maxScore = beatmap?.Versions
					.FirstOrDefault(x => x.Hash == requestedSong.SongHash.ToLower())?.Difficulties
					.FirstOrDefault(x => x.Characteristic == mappedCharacteristic && x.Difficulty == mappedDifficulty)
					?.Notes.NotesToMaxScore() ?? 0;
			}

			var accuracy = ((float) (requestedSong.UnmodifiedScore * 100) / maxScore);
			var coverImageBytes = await ScoreSaberApiService.FetchCoverImageByHash(requestedSong.SongHash).ConfigureAwait(false);
			var playerImageBytes = await ScoreSaberApiService.FetchPlayerAvatarByProfile(profile.PlayerInfo.Avatar).ConfigureAwait(false);

			await using var memoryStream = new MemoryStream();
			using (var background = new MagickImage(_backgroundImagePath))
			{
				// Cover image
				if (coverImageBytes != null)
				{
					using var coverImage = new MagickImage(coverImageBytes);
					coverImage.Resize(195, 195);
					background.Composite(coverImage, 50, 50, CompositeOperator.Over);
				}

				// Eris signature
				using (var erisSignature = new MagickImage(_erisSignaturePath))
				{
					erisSignature.Resize(100, 46);
					erisSignature.Blur(0, 0.5);
					background.Composite(erisSignature, 860, 415, CompositeOperator.Over);
				}

				// Played player signature
				if (playerImageBytes != null)
				{
					using var avatarImage = new MagickImage(playerImageBytes);
					avatarImage.Resize(new MagickGeometry {Width = 100, Height = 100});

					using var avatarLayer = new MagickImage(MagickColors.Transparent, 100, 100);
					avatarLayer.Draw(
						new DrawableFillColor(MagickColors.Black),
						new DrawableCircle(50, 50, 50, 1)
					);

					avatarLayer.Composite(avatarImage, CompositeOperator.Atop);
					background.Draw(new DrawableComposite(850, 180, CompositeOperator.Over, avatarLayer));
				}

				// PlayerName
				var playerNameSettings = new MagickReadSettings
				{
					Height = 50,
					Width = 200,
					TextGravity = Gravity.Center,
					BackgroundColor = MagickColors.Transparent,
					FontStyle = FontStyleType.Bold,
					FillColor = MagickColors.Gray
				};
				using (var playerNameCaption = new MagickImage($"label:{profile.PlayerInfo.Name}", playerNameSettings))
				{
					background.Composite(playerNameCaption, 800, 280, CompositeOperator.Over);
				}

				// Song title
				var titleCaptionSettings = new MagickReadSettings
				{
					Height = 90,
					Width = 645,
					BackgroundColor = MagickColors.Transparent,
					FontStyle = FontStyleType.Bold,
					FillColor = MagickColors.White
				};
				using (var titleCaption = new MagickImage($"caption:{requestedSong.SongName}", titleCaptionSettings))
				{
					background.Composite(titleCaption, 295, 50, CompositeOperator.Over);
				}

				// Artist(s) / Author
				var authorCaptionSettings = new MagickReadSettings
				{
					Height = 60,
					Width = 475,
					FontStyle = FontStyleType.Italic,
					BackgroundColor = MagickColors.Transparent,
					FillColor = MagickColors.Gray
				};
				using (var authorCaption = new MagickImage($"caption:{requestedSong.SongAuthorName}", authorCaptionSettings))
				{
					background.Composite(authorCaption, 295, 155, CompositeOperator.Over);
				}

				// Difficulty color
				using (var difficultyCaption = new MagickImage(requestedSong.Difficulty.ReturnDifficultyColor(), 195, 40))
				{
					background.Composite(difficultyCaption, 50, 245, CompositeOperator.Over);
				}

				// Difficulty Text
				var difficultyCaptionSettings = new MagickReadSettings
				{
					Height = 40,
					Width = 195,
					FontStyle = FontStyleType.Italic,
					BackgroundColor = MagickColors.Transparent,
					FillColor = MagickColors.White,
					TextGravity = Gravity.Center
				};
				using (var difficultyCaption = new MagickImage($"caption:{difficulty}", difficultyCaptionSettings))
				{
					background.Composite(difficultyCaption, 50, 245, CompositeOperator.Over);
				}

				// Mapper
				var mapperCaptionSettings = new MagickReadSettings
				{
					Height = 80,
					Width = 195,
					TextGravity = Gravity.Center,
					FontStyle = FontStyleType.Italic,
					BackgroundColor = MagickColors.Transparent,
					FillColor = MagickColors.Gray
				};
				using (var mapperCaption = new MagickImage($"caption:{requestedSong.LevelAuthorName}", mapperCaptionSettings))
				{
					background.Composite(mapperCaption, 50, 280, CompositeOperator.Over);
				}

				// Accuracy
				var accuracyCaptionSettings = new MagickReadSettings
				{
					Height = 50,
					Width = 225,
					TextGravity = Gravity.Center,
					FontPointsize = 30,
					BackgroundColor = MagickColors.Transparent,
					FillColor = MagickColors.Gray
				};
				using (var accuracyCaption = new MagickImage($"label:Accuracy", accuracyCaptionSettings))
				{
					background.Composite(accuracyCaption, 295, 280, CompositeOperator.Over);
				}

				accuracyCaptionSettings.FillColor = MagickColors.White;
				accuracyCaptionSettings.FontPointsize = 40;
				using (var accuracyCaption = new MagickImage($"label:{(accuracy <= 0.001f ? "n/a" : $"{accuracy:F2}")}%", accuracyCaptionSettings))
				{
					background.Composite(accuracyCaption, 295, 320, CompositeOperator.Over);
				}

				// Rank
				var rankCaptionSettings = new MagickReadSettings
				{
					Height = 50,
					Width = 150,
					TextGravity = Gravity.Center,
					FontPointsize = 30,
					BackgroundColor = MagickColors.Transparent,
					FillColor = MagickColors.Gray
				};
				using (var rankCaption = new MagickImage($"label:Rank", rankCaptionSettings))
				{
					background.Composite(rankCaption, 600, 280, CompositeOperator.Over);
				}

				rankCaptionSettings.FillColor = MagickColors.White;
				rankCaptionSettings.FontPointsize = 40;
				using (var rankCaption = new MagickImage($"label:{requestedSong.Rank}", rankCaptionSettings))
				{
					background.Composite(rankCaption, 600, 320, CompositeOperator.Over);
				}

				if (requestedSong.Weight > 0)
				{
					// Raw PP
					var rawPpCaptionSettings = new MagickReadSettings
					{
						Height = 50,
						Width = 195,
						TextGravity = Gravity.Center,
						FontPointsize = 30,
						BackgroundColor = MagickColors.Transparent,
						FillColor = MagickColors.Gray
					};
					using (var rawPpCaption = new MagickImage($"label:Raw PP", rawPpCaptionSettings))
					{
						background.Composite(rawPpCaption, 50, 380, CompositeOperator.Over);
					}

					rawPpCaptionSettings.FillColor = MagickColors.White;
					rawPpCaptionSettings.FontPointsize = 40;
					using (var rawPpCaption = new MagickImage($"label:{requestedSong.Pp:F3}", rawPpCaptionSettings))
					{
						background.Composite(rawPpCaption, 50, 420, CompositeOperator.Over);
					}

					// Weighted PP
					var weightedPpCaptionSettings = new MagickReadSettings
					{
						Height = 50,
						Width = 225,
						TextGravity = Gravity.Center,
						FontPointsize = 30,
						BackgroundColor = MagickColors.Transparent,
						FillColor = MagickColors.Gray
					};
					using (var weightedPpCaption = new MagickImage($"label:Weighted PP", weightedPpCaptionSettings))
					{
						background.Composite(weightedPpCaption, 295, 380, CompositeOperator.Over);
					}

					weightedPpCaptionSettings.FontPointsize = 40;
					weightedPpCaptionSettings.FillColor = MagickColors.White;
					using (var weightedPpCaption = new MagickImage($"label:{(requestedSong.Pp * requestedSong.Weight):F3}", weightedPpCaptionSettings))
					{
						background.Composite(weightedPpCaption, 295, 420, CompositeOperator.Over);
					}
				}

				// TimeSet
				var timeSetCaptionSettings = new MagickReadSettings
				{
					Height = 50,
					Width = 200,
					TextGravity = Gravity.Center,
					FontPointsize = 30,
					BackgroundColor = MagickColors.Transparent,
					FillColor = MagickColors.Gray
				};
				using (var timeSetCaption = new MagickImage($"label:Played", timeSetCaptionSettings))
				{
					background.Composite(timeSetCaption, 575, 380, CompositeOperator.Over);
				}

				timeSetCaptionSettings.FontPointsize = 35;
				timeSetCaptionSettings.FillColor = MagickColors.White;
				using (var timeSetCaption = new MagickImage($"label:{requestedSong.TimeSet.ToDateTimeUtc().ToShortDateString()}", timeSetCaptionSettings))
				{
					background.Composite(timeSetCaption, 575, 420, CompositeOperator.Over);
				}

				await background.WriteAsync(memoryStream).ConfigureAwait(false);
				await memoryStream.FlushAsync().ConfigureAwait(false);
				memoryStream.Seek(0, SeekOrigin.Begin);
			}

			var messageBuilder = new DiscordMessageBuilder()
				.WithContent($"Woah, {profile.PlayerInfo.Name} played:")
				.WithFile($"{profile.PlayerInfo.Name}_{SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().ToLongDateString()}.jpeg", memoryStream);
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

			var args = ctx.RawArgumentString
				.Split(" ", StringSplitOptions.TrimEntries)
				.Where(arg => !string.IsNullOrWhiteSpace(arg))
				.Where(arg => !arg.StartsWith("<@!"))
				.Where(arg => !arg.EndsWith(">"))
				.ToList();

			if (args.Any() && args[0].ExtractScoreSaberId(out scoreSaberId))
			{
				args.RemoveAt(0);
			}

			if (scoreSaberId == null)
			{
				if (ctx.Message.MentionedUsers.Any())
				{
					var mentionedUser = ctx.Message.MentionedUsers.First();
					await LookupScoreSaberLink(mentionedUser.Id.ToString()).ConfigureAwait(false);
				}
				else
				{
					await LookupScoreSaberLink(ctx.User.Id.ToString()).ConfigureAwait(false);
				}
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