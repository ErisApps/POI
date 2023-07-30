using System.Text;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ImageMagick;
using Microsoft.Extensions.Logging;
using NodaTime;
using POI.DiscordDotNet.Extensions;
using POI.DiscordDotNet.Models;
using POI.DiscordDotNet.Services.Implementations;
using POI.Persistence.Repositories;
using POI.ThirdParty.BeatSaver.Extensions;
using POI.ThirdParty.BeatSaver.Services;
using POI.ThirdParty.ScoreSaber.Extensions;
using POI.ThirdParty.ScoreSaber.Models.Profile;
using POI.ThirdParty.ScoreSaber.Models.Scores;
using POI.ThirdParty.ScoreSaber.Models.Wrappers;
using POI.ThirdParty.ScoreSaber.Services;
using Constants = POI.ThirdParty.ScoreSaber.Constants;

namespace POI.DiscordDotNet.Commands.SlashCommands.ScoreSaber;

public abstract class ScoreSaberBaseSongCommand
{
	private readonly ILogger<ScoreSaberBaseSongCommand> _logger;
	private readonly IGlobalUserSettingsRepository _globalUserSettingsRepository;
	private readonly IBeatSaverClientProvider _beatSaverClientProvider;
	private readonly PathProvider _pathProvider;

	protected IScoreSaberApiService ScoreSaberApiService { get; }

	public ScoreSaberBaseSongCommand(
		ILogger<ScoreSaberBaseSongCommand> logger,
		IScoreSaberApiService scoreSaberApiService,
		IGlobalUserSettingsRepository globalUserSettingsRepository,
		IBeatSaverClientProvider beatSaverClientProvider,
		PathProvider pathProvider)
	{
		_logger = logger;
		_globalUserSettingsRepository = globalUserSettingsRepository;
		_beatSaverClientProvider = beatSaverClientProvider;
		_pathProvider = pathProvider;

		ScoreSaberApiService = scoreSaberApiService;
	}

	public async Task Handle(InteractionContext context, long nthSongUnchecked, string? scoreSaberId, DiscordUser? discordUser)
	{
		await context.DeferAsync().ConfigureAwait(false);

		var staticArgumentValidationResult = ValidateArgumentsStatic(nthSongUnchecked, ref scoreSaberId, discordUser);
		if (!staticArgumentValidationResult.Success)
		{
			await SendError(context, staticArgumentValidationResult).ConfigureAwait(false);
			return;
		}

		if (scoreSaberId == null)
		{
			var lookupScoreSaberIdResult = await LookupScoreSaberId(context, discordUser).ConfigureAwait(false);
			if (!lookupScoreSaberIdResult.Success)
			{
				await SendError(context, lookupScoreSaberIdResult).ConfigureAwait(false);
				return;
			}

			scoreSaberId = lookupScoreSaberIdResult.Result!;
		}

		var nthSong = (uint) nthSongUnchecked - 1;

		var scoreSaberOnlineDataResult = await FetchOnlineData(scoreSaberId, nthSong).ConfigureAwait(false);
		if (!scoreSaberOnlineDataResult.Success)
		{
			await SendError(context, scoreSaberOnlineDataResult).ConfigureAwait(false);
			return;
		}

		var scoreSaberCardResponse = await GenerateScoreSaberCard(scoreSaberOnlineDataResult.Result!).ConfigureAwait(false);
		if (!scoreSaberCardResponse.Success)
		{
			await SendError(context, scoreSaberCardResponse).ConfigureAwait(false);
			return;
		}

		await using var cardImageMemoryStream = scoreSaberCardResponse.Result!.CardImageMemoryStream;
		var followupMessageBuilder = new DiscordFollowupMessageBuilder()
			.WithContent("Hii, this a work-in-progress command. Please be patient ^^")
			.AddFile(scoreSaberCardResponse.Result!.SuggestedFileName, cardImageMemoryStream);

		await context.FollowUpAsync(followupMessageBuilder).ConfigureAwait(false);
	}

	protected abstract Task<PlayerScoresWrapperDto?> FetchScorePage(string playerId, uint page);

	private static LayerResponse ValidateArgumentsStatic(long nthSong, ref string? scoreSaberId, DiscordUser? discordUser)
	{
		var layerResponse = new LayerResponse();

		// MaxValue so we can at least cast it to uint, there will be checks later on to see whether the target user has that many scores
		if (nthSong is < 1 or > uint.MaxValue)
		{
			// TODO: Can be localized based on the user's language (InteractionContext.User.Locale)
			layerResponse.AddError("Please enter a number between 1 and the total amount of songs of the target player");
		}

		if (scoreSaberId is not null && discordUser is not null)
		{
			// TODO: Can be localized based on the user's language (InteractionContext.User.Locale)
			layerResponse.AddError("Please enter a ScoreSaber ID or a Discord user, not both");
		}

		if (scoreSaberId is not null && !scoreSaberId.ExtractScoreSaberId(out scoreSaberId))
		{
			// TODO: Can be localized based on the user's language (InteractionContext.User.Locale)
			layerResponse.AddError("The entered ScoreSaber id is not valid");
		}

		return layerResponse;
	}

	private async Task<LayerResponse<string>> LookupScoreSaberId(BaseContext context, DiscordUser? discordUser)
	{
		var targetUserId = discordUser?.Id ?? context.User.Id;

		try
		{
			var userSettings = await _globalUserSettingsRepository
				.LookupSettingsByDiscordId(targetUserId)
				.ConfigureAwait(false);
			var scoreSaberId = userSettings?.ScoreSaberId;
			if (scoreSaberId is null)
			{
				return new LayerResponse<string>().AddError("Couldn't find the ScoreSaber id of the target user. Please ensure that the target user has linked their ScoreSaber account");
			}

			return LayerResponse<string>.CreateSuccess(scoreSaberId);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "An error occured while trying to find userSettings for user with discord id {DiscordUserId}", targetUserId);
			return new LayerResponse<string>().AddError("An error occured while trying to find your ScoreSaber id");
		}
	}

	private async Task<LayerResponse<ScoreSaberOnlineData>> FetchOnlineData(string scoreSaberId, uint nthSong)
	{
		var profile = await ScoreSaberApiService.FetchBasicPlayerProfile(scoreSaberId).ConfigureAwait(false);
		if (profile == null)
		{
			return new LayerResponse<ScoreSaberOnlineData>().AddError("Couldn't fetch profile");
		}

		var songPageNumber = (uint) Math.Ceiling((double) nthSong / Constants.DEFAULT_PLAYS_PER_PAGE);
		var songPage = await FetchScorePage(profile.Id, songPageNumber).ConfigureAwait(false);
		if (songPage == null)
		{
			return new LayerResponse<ScoreSaberOnlineData>().AddError("Couldn't fetch songPage");
		}

		PlayerScoreDto requestedSong;
		var localSongIndex = nthSong % Constants.DEFAULT_PLAYS_PER_PAGE;
		if (songPage.PlayerScores.Count > localSongIndex)
		{
			requestedSong = songPage.PlayerScores[(int) localSongIndex];
		}
		else
		{
			return new LayerResponse<ScoreSaberOnlineData>().AddError("The target user doesn't have this many scores just yet");
		}

		return LayerResponse<ScoreSaberOnlineData>.CreateSuccess(new ScoreSaberOnlineData(profile, requestedSong));
	}

	private record ScoreSaberOnlineData(BasicProfileDto Profile, PlayerScoreDto SongData);

	private async Task<LayerResponse<ScoreSaberCardContext>> GenerateScoreSaberCard(ScoreSaberOnlineData onlineData)
	{
		if (!onlineData.SongData.Leaderboard.DifficultyInfo.DifficultyRaw.ParseScoreSaberDifficulty(out var characteristic, out var difficulty))
		{
			return new LayerResponse<ScoreSaberCardContext>().AddError("Failed to parse ScoreSaber difficulty");
		}

		var maxScore = onlineData.SongData.Leaderboard.MaxScore;
		if (maxScore <= 0)
		{
			var beatmap = await _beatSaverClientProvider.GetClientInstance().BeatmapByHash(onlineData.SongData.Leaderboard.SongHash, skipCacheCheck: true).ConfigureAwait(false);

			var mappedCharacteristic = characteristic!.MapToBeatmapCharacteristic();
			var mappedDifficulty = difficulty!.MapToBeatSaverBeatmapDifficulty();

			maxScore = beatmap?.Versions
				.FirstOrDefault(x => x.Hash == onlineData.SongData.Leaderboard.SongHash.ToLower())?.Difficulties
				.FirstOrDefault(x => x.Characteristic == mappedCharacteristic && x.Difficulty == mappedDifficulty)
				?.Notes.NotesToMaxScore() ?? 0;
		}

		var accuracy = onlineData.SongData.Score.BaseScore * 100d / maxScore;
		var imageByteTasks = await Task.WhenAll(
				ScoreSaberApiService.FetchImageFromCdn(onlineData.SongData.Leaderboard.CoverImageUrl),
				ScoreSaberApiService.FetchImageFromCdn(onlineData.Profile.ProfilePicture))
			.ConfigureAwait(false);
		var coverImageBytes = imageByteTasks[0];
		var playerImageBytes = imageByteTasks[1];

		// TODO: Consider moving this to a separate service
		// TODO: Rewrite this using ImageSharp.Drawing
		var memoryStream = new MemoryStream();
		using var background = new MagickImage(GetBackgroundImagePath());
		// Cover image
		if (coverImageBytes != null)
		{
			using var coverImage = new MagickImage(coverImageBytes);
			coverImage.Resize(195, 195);
			background.Composite(coverImage, 50, 50, CompositeOperator.Over);
		}

		// Eris signature
		using (var erisSignature = new MagickImage(GetSignatureImagePath()))
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
		using (var playerNameCaption = new MagickImage($"label:{onlineData.Profile.Name}", playerNameSettings))
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
		using (var titleCaption = new MagickImage($"caption:{onlineData.SongData.Leaderboard.SongName}", titleCaptionSettings))
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
		using (var authorCaption = new MagickImage($"caption:{onlineData.SongData.Leaderboard.SongAuthorName}", authorCaptionSettings))
		{
			background.Composite(authorCaption, 295, 155, CompositeOperator.Over);
		}

		// Difficulty color
		using (var difficultyCaption = new MagickImage(onlineData.SongData.Leaderboard.DifficultyInfo.Difficulty.ReturnDifficultyColor(), 195, 40))
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
		using (var mapperCaption = new MagickImage($"caption:{onlineData.SongData.Leaderboard.LevelAuthorName}", mapperCaptionSettings))
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
		using (var rankCaption = new MagickImage($"label:{onlineData.SongData.Score.Rank}", rankCaptionSettings))
		{
			background.Composite(rankCaption, 600, 320, CompositeOperator.Over);
		}

		if (onlineData.SongData.Score.Weight > 0)
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
			using (var rawPpCaption = new MagickImage($"label:{onlineData.SongData.Score.Pp:F3}", rawPpCaptionSettings))
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
			using (var weightedPpCaption = new MagickImage("label:Weighted PP", weightedPpCaptionSettings))
			{
				background.Composite(weightedPpCaption, 295, 380, CompositeOperator.Over);
			}

			weightedPpCaptionSettings.FontPointsize = 40;
			weightedPpCaptionSettings.FillColor = MagickColors.White;
			using (var weightedPpCaption = new MagickImage($"label:{(onlineData.SongData.Score.Pp * onlineData.SongData.Score.Weight):F3}", weightedPpCaptionSettings))
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
		using (var timeSetCaption = new MagickImage("label:Played", timeSetCaptionSettings))
		{
			background.Composite(timeSetCaption, 575, 380, CompositeOperator.Over);
		}

		timeSetCaptionSettings.FontPointsize = 35;
		timeSetCaptionSettings.FillColor = MagickColors.White;
		using (var timeSetCaption = new MagickImage($"label:{onlineData.SongData.Score.TimeSet.ToDateTimeUtc().ToShortDateString()}", timeSetCaptionSettings))
		{
			background.Composite(timeSetCaption, 575, 420, CompositeOperator.Over);
		}

		await background.WriteAsync(memoryStream).ConfigureAwait(false);
		await memoryStream.FlushAsync().ConfigureAwait(false);
		memoryStream.Seek(0, SeekOrigin.Begin);

		return LayerResponse<ScoreSaberCardContext>.CreateSuccess(new ScoreSaberCardContext(
			$"{onlineData.Profile.Name}_{SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().ToLongDateString()}.jpeg",
			memoryStream));
	}

	private record ScoreSaberCardContext(string SuggestedFileName, MemoryStream CardImageMemoryStream);

	protected virtual string GetBackgroundImagePath() => Path.Combine(_pathProvider.AssetsPath, "poinext1.png");

	protected virtual string GetSignatureImagePath() => Path.Combine(_pathProvider.AssetsPath, "Signature-Eris.png");

	private static async Task SendError(InteractionContext context, LayerResponse response)
	{
		var interactionResponseBuilder = new DiscordEmbedBuilder()
			.WithTitle("An error occured");

		var descriptionStringBuilder = new StringBuilder("Please check the error(s) or warnings below and try again later.\nMake sure to contact Eris if the issue persists. ^^\n");
		for (var i = 0; i < response.ErrorMessages.Count; i++)
		{
			var errorMessage = response.ErrorMessages[i];
			descriptionStringBuilder
				.Append(i + 1)
				.Append(") **")
				.Append(errorMessage)
				.AppendLine("**");
		}

		interactionResponseBuilder.WithDescription(descriptionStringBuilder.ToString());

		var followupBuilder = new DiscordFollowupMessageBuilder()
			.AddEmbed(interactionResponseBuilder.Build());

		await context.FollowUpAsync(followupBuilder).ConfigureAwait(false);
	}
}