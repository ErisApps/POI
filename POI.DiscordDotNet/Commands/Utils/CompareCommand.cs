using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using ImageMagick;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NodaTime;
using POI.Core.Services;
using POI.DiscordDotNet.Commands.Beat_Saber;
using POI.DiscordDotNet.Commands.Modules;
using POI.DiscordDotNet.Extensions;
using POI.DiscordDotNet.Models.Database;
using POI.DiscordDotNet.Services;

namespace POI.DiscordDotNet.Commands.Utils
{
	public class CompareCommand : UtilCommandsModule
	{
		private const int WIDTH = 1000;
		private const int MARGIN = 50;
		private const int HEIGHT = 920;
		private const int SPACING = 25;
		private const int NAME_HEIGHT = MARGIN;
		private const int PFP_HEIGHT = NAME_HEIGHT + 70 + SPACING;
		private const int RANK_HEIGHT = PFP_HEIGHT + 150 + SPACING;

		private readonly ILogger<BaseSongCommand> _logger;
		private readonly DiscordClient _client;
		private readonly MongoDbService _mongoDbService;
		private readonly BeatSaverClientProvider _beatSaverClientProvider;
		private readonly Constants _constants;
		private readonly PathProvider _pathProvider;
		private readonly ScoreSaberService _scoreSaberService;

		public CompareCommand(ILogger<BaseSongCommand> logger, DiscordClient client, ScoreSaberService scoreSaberService, MongoDbService mongoDbService,
			BeatSaverClientProvider beatSaverClientProvider, Constants constants, PathProvider pathProvider)
		{
			_logger = logger;
			_client = client;

			_scoreSaberService = scoreSaberService;
			_mongoDbService = mongoDbService;
			_beatSaverClientProvider = beatSaverClientProvider;
			_constants = constants;
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
				await _logger.LogError(ctx, "Something went wrong while trying to parse the arguments").ConfigureAwait(false);
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

			var profile1ImageBytes = await _scoreSaberService.FetchPlayerAvatarByProfile(profile1.PlayerInfo.Avatar).ConfigureAwait(false);
			var profile2ImageBytes = await _scoreSaberService.FetchPlayerAvatarByProfile(profile2.PlayerInfo.Avatar).ConfigureAwait(false);

			var backgroundImagePath = Path.Combine(_pathProvider.AssetsPath, "poinextCompareBG.png");
			var erisSignaturePath = Path.Combine(_pathProvider.AssetsPath, "Signature-Eris.png");

			var args = new List<string[]>
			{
				new[] {$"{profile1.PlayerInfo.Rank:N0}", "Global Rank", $"{profile2.PlayerInfo.Rank:N0}"},
				new[] {$"{profile1.PlayerInfo.CountryRank:N0}", "Global Rank", $"{profile2.PlayerInfo.CountryRank:N0}"},
				new[] {$"{profile1.PlayerInfo.Pp:N1}", "PP", $"{profile2.PlayerInfo.Pp:N1}"},
				new[] {$"{Math.Floor(profile1.ScoreStats.AverageRankedAccuracy*1000)/1000}%", "AVG ACC", $"{Math.Floor(profile2.ScoreStats.AverageRankedAccuracy*1000)/1000}%"},
				new[] {$"{profile1.ScoreStats.TotalPlayCount:N0}", "Play Count", $"{profile2.ScoreStats.TotalPlayCount:N0}"},
				new[] {$"{profile1.ScoreStats.TotalRankedCount:N0}", "Ranked Play Count", $"{profile2.ScoreStats.TotalRankedCount:N0}"},
				new[] {$"{profile1.ScoreStats.TotalScore:N0}", "Score", $"{profile2.ScoreStats.TotalScore:N0}"},
				new[] {$"{profile1.ScoreStats.TotalRankedScore:N0}", "Ranked Score", $"{profile2.ScoreStats.TotalRankedScore:N0}"},
				new[] {$"{profile1TopPage.Scores[0].Pp:N2}", "Top PP Play", $"{profile2TopPage.Scores[0].Pp:N2}"},
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
				using (var playerNameCaption = new MagickImage($"label:{profile1.PlayerInfo.Name}", playerNameSettings))
				{
					background.Composite(playerNameCaption, MARGIN, NAME_HEIGHT, CompositeOperator.Over);
				}

				playerNameSettings.TextGravity = Gravity.East;
				using (var playerNameCaption = new MagickImage($"label:{profile2.PlayerInfo.Name}", playerNameSettings))
				{
					background.Composite(playerNameCaption, WIDTH - MARGIN - playerNameCaption.Width, NAME_HEIGHT, CompositeOperator.Over);
				}

				// player1 image
				if (profile1ImageBytes != null)
				{
					using var avatarImage = new MagickImage(profile1ImageBytes);
					avatarImage.Resize(new MagickGeometry { Width = 150, Height = 150 });

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
					avatarImage.Resize(new MagickGeometry { Width = 150, Height = 150 });

					using var avatarLayer = new MagickImage(MagickColors.Transparent, 150, 150);
					avatarLayer.Draw(
						new DrawableFillColor(MagickColors.Black),
						new DrawableCircle(75, 75, 75, 1)
					);

					avatarLayer.Composite(avatarImage, CompositeOperator.Atop);
					background.Draw(new DrawableComposite(WIDTH - MARGIN - avatarImage.Width, PFP_HEIGHT, CompositeOperator.Over, avatarLayer));
				}

				foreach (var item in args.Select((value, i) => new { i, value }))
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

					playerRankSettings.FillColor =
						val1 > val2 ? MagickColors.LimeGreen : Math.Abs(val1 - val2) < 0.0001 ? MagickColors.White : MagickColors.IndianRed;
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

					playerRankSettings.FillColor =
						val1 < val2 ? MagickColors.LimeGreen : Math.Abs(val1 - val2) < 0.0001 ? MagickColors.White : MagickColors.IndianRed;
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
				.WithContent($"Comparing {profile1.PlayerInfo.Name} & {profile2.PlayerInfo.Name}")
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

			//get arguments
			var args = ctx.RawArgumentString
				.Split(" ", StringSplitOptions.TrimEntries)
				.Where(arg => !string.IsNullOrWhiteSpace(arg))
				.Where(arg => !arg.StartsWith("<@!"))
				.Where(arg => !arg.EndsWith(">"))
				.ToList();

			//only 1 ScoreSaber id
			if (args.Count == 1 && args[0].ExtractScoreSaberId(out compareScoreSaberId))
			{
				args.RemoveAt(0);
				//if scoresaber id is not in arguments check mongo db for discord linked with ss
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
			}

			//2 ScoreSaber ids
			if (args.Count == 2 && args[0].ExtractScoreSaberId(out scoreSaberId) && args[1].ExtractScoreSaberId(out compareScoreSaberId))
			{
				args.RemoveAt(1);
				args.RemoveAt(0);
			}


			if (scoreSaberId == null || compareScoreSaberId == null)
			{
				return null;
			}

			return (scoreSaberId, compareScoreSaberId);
		}
	}
}