using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using ImageMagick;
using Microsoft.Extensions.Logging;
using NodaTime;
using POI.DiscordDotNet.Commands.Modules.ChatCommands;
using POI.DiscordDotNet.Extensions;
using POI.Persistence.Repositories;
using POI.ThirdParty.BeatSaver.Extensions;
using POI.ThirdParty.BeatSaver.Services;
using POI.ThirdParty.BeatSavior.Models;
using POI.ThirdParty.BeatSavior.Services;
using POI.ThirdParty.ScoreSaber;
using POI.ThirdParty.ScoreSaber.Extensions;
using POI.ThirdParty.ScoreSaber.Models.Profile;
using POI.ThirdParty.ScoreSaber.Models.Scores;
using POI.ThirdParty.ScoreSaber.Models.Wrappers;
using POI.ThirdParty.ScoreSaber.Services;

namespace POI.DiscordDotNet.Commands.BeatSaber
{
	public abstract class BaseSongCommand : BeatSaberCommandsModule
	{
		private readonly ILogger<BaseSongCommand> _logger;
		private readonly string _backgroundImagePath;
		private readonly string _erisSignaturePath;
		private readonly IGlobalUserSettingsRepository _globalUserSettingsRepository;
		private readonly IBeatSaverClientProvider _beatSaverClientProvider;

		protected readonly IScoreSaberApiService ScoreSaberApiService;
		protected readonly IBeatSaviorApiService BeatSaviorApiService;

		private const int WIDTH = 1024;
		private const int MARGIN = 35;

		protected BaseSongCommand(ILogger<BaseSongCommand> logger, IScoreSaberApiService scoreSaberApiService, IGlobalUserSettingsRepository globalUserSettingsRepository,
			IBeatSaverClientProvider beatSaverClientProvider, string backgroundImagePath, string erisSignaturePath, IBeatSaviorApiService beatSaviorApiService)
		{
			_logger = logger;

			ScoreSaberApiService = scoreSaberApiService;
			BeatSaviorApiService = beatSaviorApiService;

			_globalUserSettingsRepository = globalUserSettingsRepository;
			_beatSaverClientProvider = beatSaverClientProvider;
			_backgroundImagePath = backgroundImagePath;
			_erisSignaturePath = erisSignaturePath;
		}

		protected abstract Task<PlayerScoresWrapperDto?> FetchScorePage(string playerId, uint page);

		// ReSharper disable once CognitiveComplexity
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

			var songPageNumber = (uint) Math.Ceiling((double) nthSong / Constants.DEFAULT_PLAYS_PER_PAGE);
			var songPage = await FetchScorePage(profile.Id, songPageNumber).ConfigureAwait(false);
			if (songPage == null)
			{
				await _logger.LogError(ctx, "Couldn't fetch songPage").ConfigureAwait(false);
				return;
			}

			PlayerScoreDto requestedSong;
			var localSongIndex = --nthSong % Constants.DEFAULT_PLAYS_PER_PAGE;
			if (songPage.PlayerScores.Count > localSongIndex)
			{
				requestedSong = songPage.PlayerScores[localSongIndex];
			}
			else
			{
				await _logger.LogError(ctx, "The user doesn't have this many scores just yet").ConfigureAwait(false);
				return;
			}

			if (!requestedSong.Leaderboard.DifficultyInfo.DifficultyRaw.ParseScoreSaberDifficulty(out var characteristic, out var difficulty))
			{
				await _logger.LogError(ctx, "Failed to parse ScoreSaber difficulty").ConfigureAwait(false);
				return;
			}

			var maxScore = requestedSong.Leaderboard.MaxScore;
			if (maxScore <= 0)
			{
				var beatmap = await _beatSaverClientProvider.GetClientInstance().BeatmapByHash(requestedSong.Leaderboard.SongHash, skipCacheCheck: true).ConfigureAwait(false);

				var mappedCharacteristic = characteristic!.MapToBeatmapCharacteristic();
				var mappedDifficulty = difficulty!.MapToBeatSaverBeatmapDifficulty();

				maxScore = beatmap?.Versions
					.FirstOrDefault(x => x.Hash == requestedSong.Leaderboard.SongHash.ToLower())?.Difficulties
					.FirstOrDefault(x => x.Characteristic == mappedCharacteristic && x.Difficulty == mappedDifficulty)
					?.Notes.NotesToMaxScore() ?? 0;
			}

			var accuracy = ((float) (requestedSong.Score.BaseScore * 100) / maxScore);
			var coverImageBytes = await ScoreSaberApiService.FetchImageFromCdn(requestedSong.Leaderboard.CoverImageUrl).ConfigureAwait(false);
			var playerImageBytes = await ScoreSaberApiService.FetchImageFromCdn(profile.ProfilePicture).ConfigureAwait(false);

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
					avatarImage.Resize(new MagickGeometry { Width = 100, Height = 100 });

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
				using (var playerNameCaption = new MagickImage($"label:{profile.Name}", playerNameSettings))
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
				using (var titleCaption = new MagickImage($"caption:{requestedSong.Leaderboard.SongName}", titleCaptionSettings))
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
				using (var authorCaption = new MagickImage($"caption:{requestedSong.Leaderboard.SongAuthorName}", authorCaptionSettings))
				{
					background.Composite(authorCaption, 295, 155, CompositeOperator.Over);
				}

				// Difficulty color
				using (var difficultyCaption = new MagickImage(requestedSong.Leaderboard.DifficultyInfo.Difficulty.ReturnDifficultyColor(), 195, 40))
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
				using (var mapperCaption = new MagickImage($"caption:{requestedSong.Leaderboard.LevelAuthorName}", mapperCaptionSettings))
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
				using (var rankCaption = new MagickImage($"label:{requestedSong.Score.Rank}", rankCaptionSettings))
				{
					background.Composite(rankCaption, 600, 320, CompositeOperator.Over);
				}

				if (requestedSong.Score.Weight > 0)
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
					using (var rawPpCaption = new MagickImage($"label:{requestedSong.Score.Pp:F3}", rawPpCaptionSettings))
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
					using (var weightedPpCaption = new MagickImage($"label:{(requestedSong.Score.Pp * requestedSong.Score.Weight):F3}", weightedPpCaptionSettings))
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
				using (var timeSetCaption = new MagickImage($"label:{requestedSong.Score.TimeSet.ToDateTimeUtc().ToShortDateString()}", timeSetCaptionSettings))
				{
					background.Composite(timeSetCaption, 575, 420, CompositeOperator.Over);
				}

				await background.WriteAsync(memoryStream).ConfigureAwait(false);
				await memoryStream.FlushAsync().ConfigureAwait(false);
				memoryStream.Seek(0, SeekOrigin.Begin);
			}

			var messageBuilder = new DiscordMessageBuilder()
				.WithContent($"Woah, {profile.Name} played:")
				.WithFile($"{profile.Name}_{SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().ToLongDateString()}.jpeg", memoryStream);
			await ctx.Message
				.RespondAsync(messageBuilder)
				.ConfigureAwait(false);


			// Getting data for the second image
			var beatSaviorProfileData = await BeatSaviorApiService.FetchBeatSaviorPlayerData(scoreSaberId).ConfigureAwait(false);

			//BeatSavior data found! (Making the second image)
			if (beatSaviorProfileData != null)
			{
				var beatSaviorMatchingPlays = beatSaviorProfileData
					.Where(song => requestedSong.Leaderboard.SongHash.Equals(song.SongId, StringComparison.InvariantCultureIgnoreCase)
					               && requestedSong.Leaderboard.DifficultyInfo.Difficulty == song.SongDifficultyRank
					               && !string.IsNullOrWhiteSpace(song.GameMode)
					               && requestedSong.Leaderboard.DifficultyInfo.GameMode.Contains(song.GameMode))
					.OrderByDescending(song => song.Trackers.ScoreTracker.Score)
					.ToList();

				if (beatSaviorMatchingPlays.Any())
				{
					await SendBeatSaviorMemoryStream(ctx, profile, beatSaviorMatchingPlays.FirstOrDefault()).ConfigureAwait(false);
				}
			}
		}

		private async Task SendBeatSaviorMemoryStream(CommandContext ctx, BasicProfileDto profile, SongDataDto beatSaviorSongData)
		{
			await using var memoryStream = new MemoryStream();

			using (var background = new MagickImage(_backgroundImagePath))
			{
				var hitTracker = beatSaviorSongData.Trackers.HitTracker;
				var accuracyTracker = beatSaviorSongData.Trackers.AccuracyTracker;
				var winTracker = beatSaviorSongData.Trackers.WinTracker;

				// Run Stats
				var runStatsSettings = new MagickReadSettings
				{
					Height = 70,
					Width = WIDTH / 5 - MARGIN,
					BackgroundColor = MagickColors.Transparent,
					FontStyle = FontStyleType.Bold,
					FillColor = MagickColors.White,
					TextGravity = Gravity.Center,
					FontPointsize = 40
				};

				var runStatsTitleSettings = new MagickReadSettings
				{
					Height = 70,
					Width = WIDTH / 5 - MARGIN,
					BackgroundColor = MagickColors.Transparent,
					FillColor = MagickColors.Gray,
					TextGravity = Gravity.Center,
					FontPointsize = 30
				};


				//LeftHand
				using (var leftHandCircle = new MagickImage(MagickColors.Transparent, 512, 512))
				{
					new Drawables()
						// Add an ellipse
						.StrokeColor(new MagickColor("#a82020"))
						.StrokeWidth(12)
						.FillColor(MagickColors.Transparent)
						.Ellipse(120, 120, 100, 100, -90, accuracyTracker.AccLeft / 115 * 360 - 90)
						.Draw(leftHandCircle);
					background.Composite(leftHandCircle, WIDTH / 4 - 15, 135, CompositeOperator.Over);
				}

				//Cut
				using (var caption = new MagickImage("label:Cut", runStatsTitleSettings))
				{
					background.Composite(caption, MARGIN + WIDTH / 4 - 15, 190, CompositeOperator.Over);
				}

				using (var caption = new MagickImage($"label:{accuracyTracker.AccLeft:f2}", runStatsSettings))
				{
					background.Composite(caption, MARGIN + WIDTH / 4 - 15, 230, CompositeOperator.Over);
				}

				//Pre cut
				using (var caption = new MagickImage("label:Pre Cut", runStatsTitleSettings))
				{
					background.Composite(caption, MARGIN + 0, 110, CompositeOperator.Over);
				}

				using (var caption = new MagickImage($"label:{accuracyTracker.LeftAverageCut[0]:f2}", runStatsSettings))
				{
					background.Composite(caption, MARGIN + 0, 155, CompositeOperator.Over);
				}

				//Post cut
				using (var caption = new MagickImage("label:Post Cut", runStatsTitleSettings))
				{
					background.Composite(caption, MARGIN + 0, 200, CompositeOperator.Over);
				}

				using (var caption = new MagickImage($"label:{accuracyTracker.LeftAverageCut[2]:f2}", runStatsSettings))
				{
					background.Composite(caption, MARGIN + 0, 245, CompositeOperator.Over);
				}

				//Acc cut
				using (var caption = new MagickImage("label:Acc Cut", runStatsTitleSettings))
				{
					background.Composite(caption, MARGIN + 0, 290, CompositeOperator.Over);
				}

				using (var caption = new MagickImage($"label:{accuracyTracker.LeftAverageCut[1]:f2}", runStatsSettings))
				{
					background.Composite(caption, MARGIN + 0, 335, CompositeOperator.Over);
				}

				//Pre swing
				using (var caption = new MagickImage("label:Pre Swing", runStatsTitleSettings))
				{
					background.Composite(caption, MARGIN + 0, 380, CompositeOperator.Over);
				}

				using (var caption = new MagickImage($"label:{(accuracyTracker.LeftPreswing * 100):f2}%", runStatsSettings))
				{
					background.Composite(caption, MARGIN + 0, 425, CompositeOperator.Over);
				}

				//Post swing
				using (var caption = new MagickImage("label:Post Swing", runStatsTitleSettings))
				{
					background.Composite(caption, MARGIN + WIDTH / 4, 380, CompositeOperator.Over);
				}

				using (var caption = new MagickImage($"label:{(accuracyTracker.LeftPostswing * 100):f2}%", runStatsSettings))
				{
					background.Composite(caption, MARGIN + WIDTH / 4, 425, CompositeOperator.Over);
				}


				//RightHand
				using (var rightHandCircle = new MagickImage(MagickColors.Transparent, 512, 512))
				{
					new Drawables()
						// Add an ellipse
						.StrokeColor(new MagickColor("#2064a8"))
						.StrokeWidth(12)
						.FillColor(MagickColors.Transparent)
						.Ellipse(120, 120, 100, 100, -90, accuracyTracker.AccRight / 115 * 360 - 90)
						.Draw(rightHandCircle);
					background.Composite(rightHandCircle, WIDTH / 4 * 2 + 15, 135, CompositeOperator.Over);
				}

				//Cut
				using (var caption = new MagickImage("label:Cut", runStatsTitleSettings))
				{
					background.Composite(caption, MARGIN + WIDTH / 4 * 2 + 15, 190, CompositeOperator.Over);
				}

				using (var caption = new MagickImage($"label:{accuracyTracker.AccRight:f2}", runStatsSettings))
				{
					background.Composite(caption, MARGIN + WIDTH / 4 * 2 + 15, 230, CompositeOperator.Over);
				}

				//Pre cut
				using (var caption = new MagickImage("label:Pre Cut", runStatsTitleSettings))
				{
					background.Composite(caption, MARGIN + WIDTH / 4 * 3, 100, CompositeOperator.Over);
				}

				using (var caption = new MagickImage($"label:{accuracyTracker.RightAverageCut[0]:f2}", runStatsSettings))
				{
					background.Composite(caption, MARGIN + WIDTH / 4 * 3, 155, CompositeOperator.Over);
				}

				//Post cut
				using (var caption = new MagickImage("label:Post Cut", runStatsTitleSettings))
				{
					background.Composite(caption, MARGIN + WIDTH / 4 * 3, 200, CompositeOperator.Over);
				}

				using (var caption = new MagickImage($"label:{accuracyTracker.RightAverageCut[2]:f2}", runStatsSettings))
				{
					background.Composite(caption, MARGIN + WIDTH / 4 * 3, 245, CompositeOperator.Over);
				}

				//Acc cut
				using (var caption = new MagickImage("label:Acc Cut", runStatsTitleSettings))
				{
					background.Composite(caption, MARGIN + WIDTH / 4 * 3, 290, CompositeOperator.Over);
				}

				using (var caption = new MagickImage($"label:{accuracyTracker.RightAverageCut[1]:f2}", runStatsSettings))
				{
					background.Composite(caption, MARGIN + WIDTH / 4 * 3, 335, CompositeOperator.Over);
				}

				//Pre swing
				using (var caption = new MagickImage("label:Pre Swing", runStatsTitleSettings))
				{
					background.Composite(caption, MARGIN + WIDTH / 4 * 3, 380, CompositeOperator.Over);
				}

				using (var caption = new MagickImage($"label:{(accuracyTracker.RightPreswing * 100):f2}%", runStatsSettings))
				{
					background.Composite(caption, MARGIN + WIDTH / 4 * 3, 425, CompositeOperator.Over);
				}

				//Post swing
				using (var caption = new MagickImage("label:Post Swing", runStatsTitleSettings))
				{
					background.Composite(caption, MARGIN + WIDTH / 4 * 2, 380, CompositeOperator.Over);
				}

				using (var caption = new MagickImage($"label:{(accuracyTracker.RightPostswing * 100):f2}%", runStatsSettings))
				{
					background.Composite(caption, MARGIN + WIDTH / 4 * 2, 425, CompositeOperator.Over);
				}


				//--------------------------------------------------------------------------------
				//Rating
				using (var caption = new MagickImage("label:Rating", runStatsTitleSettings))
				{
					background.Composite(caption, MARGIN + 0, 0, CompositeOperator.Over);
				}

				using (var caption = new MagickImage($"label:{winTracker.Rank}", runStatsSettings))
				{
					background.Composite(caption, MARGIN + 0, 45, CompositeOperator.Over);
				}

				//Combo
				using (var caption = new MagickImage("label:Combo", runStatsTitleSettings))
				{
					background.Composite(caption, MARGIN + WIDTH / 4, 0, CompositeOperator.Over);
				}

				using (var caption = new MagickImage($"label:{hitTracker.MaxCombo}", runStatsSettings))
				{
					background.Composite(caption, MARGIN + WIDTH / 4, 45, CompositeOperator.Over);
				}

				//Misses
				var missTitleSettings = runStatsTitleSettings;
				var missSettings = runStatsSettings;
				var isFullCombo = hitTracker.BadCuts == 0 && hitTracker.MissedNotes == 0 && hitTracker.BombHit == 0 && hitTracker.NbOfWallHit == 0;
				if (isFullCombo)
				{
					missSettings.FillColor = MagickColors.Gold;
				}

				using (var playerRankCaption = new MagickImage("label:Misses", missTitleSettings))
				{
					background.Composite(playerRankCaption, MARGIN + WIDTH / 4 * 2, 0, CompositeOperator.Over);
				}

				using (var playerRankCaption = new MagickImage($"label:{(isFullCombo ? "FC" : (hitTracker.MissedNotes + hitTracker.BadCuts))}", missSettings))
				{
					background.Composite(playerRankCaption, MARGIN + WIDTH / 4 * 2, 45, CompositeOperator.Over);
				}

				//Pauses

				missSettings.FillColor = MagickColors.White;
				using (var playerRankCaption = new MagickImage("label:Pauses", runStatsTitleSettings))
				{
					background.Composite(playerRankCaption, MARGIN + WIDTH / 4 * 3, 0, CompositeOperator.Over);
				}

				using (var playerRankCaption = new MagickImage($"label:{winTracker.NumberOfPauses}", runStatsSettings))
				{
					background.Composite(playerRankCaption, MARGIN + WIDTH / 4 * 3, 45, CompositeOperator.Over);
				}


				await background.WriteAsync(memoryStream).ConfigureAwait(false);
				await memoryStream.FlushAsync().ConfigureAwait(false);
				memoryStream.Seek(0, SeekOrigin.Begin);
			}

			var messageBuilder = new DiscordMessageBuilder()
				.WithFile($"{profile.Name}_BeatSavior_{SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().ToLongDateString()}.jpeg", memoryStream);

			await ctx.Message
				.Channel.SendMessageAsync(messageBuilder)
				.ConfigureAwait(false);
		}

		private async Task<(string, int)?> ExtractArguments(CommandContext ctx)
		{
			string? scoreSaberId = null;

			async Task LookupScoreSaberLink(ulong discordId)
			{
				try
				{
					var userSettings = await _globalUserSettingsRepository
						.LookupSettingsByDiscordId(discordId)
						.ConfigureAwait(false);
					scoreSaberId = userSettings?.ScoreSaberId;
				}
				catch (Exception)
				{
					_logger.LogWarning("Couldn't find userSettings for user with discord id {DiscordUserId}", discordId);
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
					await LookupScoreSaberLink(mentionedUser.Id).ConfigureAwait(false);
				}
				else
				{
					await LookupScoreSaberLink(ctx.User.Id).ConfigureAwait(false);
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