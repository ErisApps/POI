using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using ImageMagick;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NodaTime;
using POI.Core.Services;
using POI.DiscordDotNet.Commands.Modules.ChatCommands;
using POI.DiscordDotNet.Extensions;
using POI.DiscordDotNet.Repositories;
using POI.DiscordDotNet.Services;

namespace POI.DiscordDotNet.Commands.BeatSaber
{
	[UsedImplicitly]
	public class CompareCommand : BeatSaberCommandsModule
	{
		private const int WIDTH = 1000;
		private const int MARGIN = 50;
		private const int HEIGHT = 920;
		private const int SPACING = 25;
		private const int NAME_HEIGHT = MARGIN;
		private const int PFP_HEIGHT = NAME_HEIGHT + 70 + SPACING;
		private const int RANK_HEIGHT = PFP_HEIGHT + 150 + SPACING;

		private readonly ILogger<CompareCommand> _logger;
		private readonly GlobalUserSettingsRepository _globalUserSettingsRepository;
		private readonly PathProvider _pathProvider;
		private readonly ScoreSaberApiService _scoreSaberService;

		public CompareCommand(ILogger<CompareCommand> logger, ScoreSaberApiService scoreSaberService, GlobalUserSettingsRepository globalUserSettingsRepository, PathProvider pathProvider)
		{
			_logger = logger;

			_scoreSaberService = scoreSaberService;
			_globalUserSettingsRepository = globalUserSettingsRepository;
			_pathProvider = pathProvider;
		}

		[Command("comparecommand")]
		[Aliases("cp", "compareprofile", "compare")]
		public async Task Handle(CommandContext ctx, [RemainingText] string _)
		{
			await ctx.TriggerTypingAsync().ConfigureAwait(false);

			var arguments = await ExtractArguments(ctx).ConfigureAwait(false);
			if (arguments == null)
			{
				var errorMessage = await ctx
					.RespondAsync(
						"Try using the correct arguments, something like this: ```poinext cp <Player 1 (scoresaberId or mention)> <Player 2 (scoresaberId or mention, optional if you are linked)>```")
					.ConfigureAwait(false);

				_logger.LogError("{ErrorMessage}", $"{ctx.User.Username} issued the compare command but failed to enter the correct arguments!");

				await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
				await errorMessage.DeleteAsync().ConfigureAwait(false);

				return;
			}

			var (scoreSaberId, compareScoreSaberId) = arguments.Value;

			var profile1 = await _scoreSaberService.FetchFullPlayerProfile(scoreSaberId).ConfigureAwait(false);
			if (profile1 == null)
			{
				await _logger.LogError(ctx, $"Couldn't fetch profile {scoreSaberId}").ConfigureAwait(false);
				return;
			}

			var profile2 = await _scoreSaberService.FetchFullPlayerProfile(compareScoreSaberId).ConfigureAwait(false);
			if (profile2 == null)
			{
				await _logger.LogError(ctx, $"Couldn't fetch profile {compareScoreSaberId}").ConfigureAwait(false);
				return;
			}

			var profile1TopPage = await _scoreSaberService.FetchTopSongsScorePage(scoreSaberId, 0).ConfigureAwait(false);
			if (profile1TopPage == null)
			{
				await _logger.LogError(ctx, $"Couldn't fetch profile top scores {scoreSaberId}").ConfigureAwait(false);
				return;
			}

			var profile2TopPage = await _scoreSaberService.FetchTopSongsScorePage(compareScoreSaberId, 0).ConfigureAwait(false);
			if (profile2TopPage == null)
			{
				await _logger.LogError(ctx, $"Couldn't fetch profile top scores {compareScoreSaberId}").ConfigureAwait(false);
				return;
			}

			var profile1ImageBytes = await _scoreSaberService.FetchImageFromCdn(profile1.ProfilePicture).ConfigureAwait(false);
			var profile2ImageBytes = await _scoreSaberService.FetchImageFromCdn(profile2.ProfilePicture).ConfigureAwait(false);

			var backgroundImagePath = Path.Combine(_pathProvider.AssetsPath, "poinextCompareBG.png");
			var erisSignaturePath = Path.Combine(_pathProvider.AssetsPath, "Signature-Eris.png");

			var args = new[]
			{
				new[] {profile1.Rank.ToString("N0"), "Global Rank", profile2.Rank.ToString("N0")},
				new[] {profile1.CountryRank.ToString("N0"), "Country Rank", profile2.CountryRank.ToString("N0")},
				new[] {profile1.Pp.ToString("N2"), "PP", profile2.Pp.ToString("N2")},
				new[] {$"{(Math.Floor(profile1.ScoreStats.AverageRankedAccuracy * 1000) / 1000):F2}%", "AVG ACC", $"{(Math.Floor(profile2.ScoreStats.AverageRankedAccuracy * 1000) / 1000):F2}%"},
				new[] {profile1.ScoreStats.TotalPlayCount.ToString("N0"), "Play Count", profile2.ScoreStats.TotalPlayCount.ToString("N0")},
				new[] {profile1.ScoreStats.TotalRankedCount.ToString("N0"), "Ranked Play Count", profile2.ScoreStats.TotalRankedCount.ToString("N0")},
				new[] {profile1.ScoreStats.TotalScore.ToString("N0"), "Score", profile2.ScoreStats.TotalScore.ToString("N0")},
				new[] {profile1.ScoreStats.TotalRankedScore.ToString("N0"), "Ranked Score", profile2.ScoreStats.TotalRankedScore.ToString("N0")},
				new[] {profile1TopPage.PlayerScores[0].Score.Pp.ToString("N2"), "Top PP Play", profile2TopPage.PlayerScores[0].Score.Pp.ToString("N2")}
			};

			await using var memoryStream = new MemoryStream();
			using (var background = new MagickImage(backgroundImagePath))
			{
				// Eris signature
				using (var erisSignature = new MagickImage(erisSignaturePath))
				{
					erisSignature.Resize(100, 46);
					erisSignature.Blur(0, 0.5);
					background.Composite(erisSignature, WIDTH - MARGIN - erisSignature.Width, HEIGHT - MARGIN - erisSignature.Height, CompositeOperator.Over);
				}

				// PlayerNames
				var playerNameSettings = new MagickReadSettings
				{
					Height = 70,
					Width = WIDTH / 2 - MARGIN,
					BackgroundColor = MagickColors.Transparent,
					FontStyle = FontStyleType.Bold,
					FillColor = MagickColors.White,
					TextGravity = Gravity.West
				};

				using (var playerNameCaption = new MagickImage($"label:{profile1.Name}", playerNameSettings))
				{
					background.Composite(playerNameCaption, MARGIN, NAME_HEIGHT, CompositeOperator.Over);
				}

				playerNameSettings.TextGravity = Gravity.East;
				using (var playerNameCaption = new MagickImage($"label:{profile2.Name}", playerNameSettings))
				{
					background.Composite(playerNameCaption, WIDTH - MARGIN - playerNameCaption.Width, NAME_HEIGHT, CompositeOperator.Over);
				}

				// player1 image
				if (profile1ImageBytes != null)
				{
					using var avatarImage = new MagickImage(profile1ImageBytes);
					avatarImage.Resize(new MagickGeometry {Width = 150, Height = 150});

					using var avatarLayer = new MagickImage(MagickColors.Transparent, 150, 150);
					avatarLayer.Draw(
						new DrawableFillColor(MagickColors.Black),
						new DrawableCircle(75, 75, 75, 1)
					);

					avatarLayer.Composite(avatarImage, CompositeOperator.Atop);
					background.Draw(new DrawableComposite(MARGIN, PFP_HEIGHT, CompositeOperator.Over, avatarLayer));
				}

				// player2 image
				if (profile2ImageBytes != null)
				{
					using var avatarImage = new MagickImage(profile2ImageBytes);
					avatarImage.Resize(new MagickGeometry {Width = 150, Height = 150});

					using var avatarLayer = new MagickImage(MagickColors.Transparent, 150, 150);
					avatarLayer.Draw(
						new DrawableFillColor(MagickColors.Black),
						new DrawableCircle(75, 75, 75, 1)
					);

					avatarLayer.Composite(avatarImage, CompositeOperator.Atop);
					background.Draw(new DrawableComposite(WIDTH - MARGIN - avatarImage.Width, PFP_HEIGHT, CompositeOperator.Over, avatarLayer));
				}

				foreach (var item in args.Select((value, i) => new {i, value}))
				{
					var arg = item.value;
					var index = item.i;

					var val1 = double.Parse(arg[0].Replace("%", ""));
					var val2 = double.Parse(arg[2].Replace("%", ""));

					if (index < 2)
					{
						val1 = -val1;
						val2 = -val2;
					}

					var playerRankSettings = new MagickReadSettings
					{
						Height = 50,
						Width = 300,
						BackgroundColor = MagickColors.Transparent,
						FontStyle = FontStyleType.Bold,
						FillColor = MagickColors.White,
						TextGravity = Gravity.West
					};

					playerRankSettings.FillColor = val1 > val2 ? MagickColors.LimeGreen : Math.Abs(val1 - val2) < 0.0001 ? MagickColors.White : MagickColors.IndianRed;
					using (var playerRankCaption = new MagickImage($"label:{arg[0]}", playerRankSettings))
					{
						background.Composite(playerRankCaption, MARGIN, RANK_HEIGHT + index * 50, CompositeOperator.Over);
					}

					playerRankSettings.FillColor = MagickColors.White;
					playerRankSettings.TextGravity = Gravity.Center;
					using (var playerRankCaption = new MagickImage($"label:{arg[1]}", playerRankSettings))
					{
						background.Composite(playerRankCaption, WIDTH / 2 - playerRankCaption.Width / 2, RANK_HEIGHT + index * 50, CompositeOperator.Over);
					}

					playerRankSettings.FillColor = val1 < val2 ? MagickColors.LimeGreen : Math.Abs(val1 - val2) < 0.0001 ? MagickColors.White : MagickColors.IndianRed;
					playerRankSettings.TextGravity = Gravity.East;
					using (var playerRankCaption = new MagickImage($"label:{arg[2]}", playerRankSettings))
					{
						background.Composite(playerRankCaption, WIDTH - MARGIN - playerRankCaption.Width, RANK_HEIGHT + index * 50, CompositeOperator.Over);
					}
				}

				//Don't comment this next time...
				await background.WriteAsync(memoryStream).ConfigureAwait(false);
				await memoryStream.FlushAsync().ConfigureAwait(false);
				memoryStream.Seek(0, SeekOrigin.Begin);
			}

			var messageBuilder = new DiscordMessageBuilder()
				.WithContent($"Comparing {profile1.Name} & {profile2.Name}")
				.WithFile($"{SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().ToLongDateString()}.jpeg", memoryStream);
			await ctx.Message
				.RespondAsync(messageBuilder)
				.ConfigureAwait(false);
		}

		private async Task<(string, string)?> ExtractArguments(CommandContext ctx)
		{
			string? scoreSaberId = null;
			string? compareScoreSaberId = null;

			//lookup sender ScoreSaber in mongoDB via discord
			async Task<string?> LookupScoreSaberLink(string discordId)
			{
				try
				{
					var userScoreLinks = await _globalUserSettingsRepository
						.LookupSettingsByDiscordId(discordId)
						.ConfigureAwait(false);
					return userScoreLinks?.AccountLinks.ScoreSaberId;
				}
				catch (Exception)
				{
					_logger.LogWarning("Couldn't find userSettings for user with id {DiscordId}", discordId);
					return null;
				}
			}

			//get arguments
			var args = ctx.RawArgumentString
				.Split(" ", StringSplitOptions.TrimEntries)
				.Where(arg => !string.IsNullOrWhiteSpace(arg))
				.Where(arg => !arg.StartsWith("<@!"))
				.Where(arg => !arg.EndsWith(">"))
				.ToList();

			switch (ctx.Message.MentionedUsers.Count)
			{
				case 0:
					switch (args.Count)
					{
						case 1 when args[0].ExtractScoreSaberId(out compareScoreSaberId):
							scoreSaberId = await LookupScoreSaberLink(ctx.Message.Author.Id.ToString()).ConfigureAwait(false);
							break;
						case 2:
							args[0].ExtractScoreSaberId(out scoreSaberId);
							args[1].ExtractScoreSaberId(out compareScoreSaberId);
							break;
						default:
							return null;
					}

					break;
				case 1:
					switch (args.Count)
					{
						case 0:
							scoreSaberId = await LookupScoreSaberLink(ctx.Message.Author.Id.ToString()).ConfigureAwait(false);
							compareScoreSaberId = await LookupScoreSaberLink(ctx.Message.MentionedUsers[0].Id.ToString()).ConfigureAwait(false);
							break;
						case 1 when args[0].ExtractScoreSaberId(out compareScoreSaberId):
							scoreSaberId = await LookupScoreSaberLink(ctx.Message.MentionedUsers[0].Id.ToString()).ConfigureAwait(false);
							break;
					}

					break;
				case 2:
					scoreSaberId = await LookupScoreSaberLink(ctx.Message.MentionedUsers[0].Id.ToString()).ConfigureAwait(false);
					compareScoreSaberId = await LookupScoreSaberLink(ctx.Message.MentionedUsers[1].Id.ToString()).ConfigureAwait(false);
					break;
				default:
					return null;
			}

			if (scoreSaberId == null || compareScoreSaberId == null)
			{
				return null;
			}

			return (scoreSaberId, compareScoreSaberId);
		}
	}
}