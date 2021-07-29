using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using POI.Core.Services;
using POI.DiscordDotNet.Commands.Beat_Saber;
using POI.DiscordDotNet.Commands.Modules;
using POI.DiscordDotNet.Extensions;
using POI.DiscordDotNet.Models.Database;
using POI.DiscordDotNet.Services;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using DSharpPlus.Entities;
using ImageMagick;
using NodaTime;

namespace POI.DiscordDotNet.Commands.Utils
{
	public class CompareCommand : UtilCommandsModule
	{
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

			var profile1TopPage = await _scoreSaberService.FetchTopSongsScorePage(scoreSaberId).ConfigureAwait(false);
			if (profile1TopPage == null)
			{
				await _logger.LogError(ctx, $"Couldn't fetch profile top scores {scoreSaberId}").ConfigureAwait(false);
				return;
			}

			var profile2TopPage = await _scoreSaberService.FetchTopSongsScorePage(compareScoreSaberId).ConfigureAwait(false);
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

			const int width = 1000,
				margin = 50,
				height = 920,
				spacing = 25,
				nameHeight = margin,
				pfpHeight = nameHeight + 70 + spacing,
				rankHeight = pfpHeight + 150 + spacing;


			await using var memoryStream = new MemoryStream();
			using (var background = new MagickImage(backgroundImagePath))
			{
				// Eris signature
				using (var erisSignature = new MagickImage(erisSignaturePath))
				{
					erisSignature.Resize(100, 46);
					erisSignature.Blur(0, 0.5);
					background.Composite(erisSignature, width - margin - erisSignature.Width, height - margin - erisSignature.Height, CompositeOperator.Over);
				}


				// PlayerNames
				var playerNameSettings = new MagickReadSettings
				{
					Height = 70,
					Width = width/2-margin,
					BackgroundColor = MagickColors.Transparent,
					FontStyle = FontStyleType.Bold,
					FillColor = MagickColors.White,
					TextGravity = Gravity.West
				};
				using (var playerNameCaption = new MagickImage($"label:{profile1.PlayerInfo.Name}", playerNameSettings))
				{
					background.Composite(playerNameCaption, margin, nameHeight, CompositeOperator.Over);
				}

				playerNameSettings.TextGravity = Gravity.East;
				using (var playerNameCaption = new MagickImage($"label:{profile2.PlayerInfo.Name}", playerNameSettings))
				{
					background.Composite(playerNameCaption, width - margin - playerNameCaption.Width, nameHeight, CompositeOperator.Over);
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
					background.Draw(new DrawableComposite(margin, pfpHeight, CompositeOperator.Over, avatarLayer));
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
					background.Draw(new DrawableComposite(width - margin - avatarImage.Width, pfpHeight, CompositeOperator.Over, avatarLayer));
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

					playerRankSettings.FillColor =
						val1 > val2 ? MagickColors.LimeGreen : Math.Abs(val1 - val2) < 0.0001 ? MagickColors.White : MagickColors.IndianRed;
					using (var playerRankCaption = new MagickImage($"label:{arg[0]}", playerRankSettings))
					{
						background.Composite(playerRankCaption, margin, rankHeight + index * 50, CompositeOperator.Over);
					}

					playerRankSettings.FillColor = MagickColors.White;
					playerRankSettings.TextGravity = Gravity.Center;
					using (var playerRankCaption = new MagickImage($"label:{arg[1]}", playerRankSettings))
					{
						background.Composite(playerRankCaption, width / 2 - playerRankCaption.Width / 2, rankHeight + index * 50, CompositeOperator.Over);
					}

					playerRankSettings.FillColor =
						val1 < val2 ? MagickColors.LimeGreen : Math.Abs(val1 - val2) < 0.0001 ? MagickColors.White : MagickColors.IndianRed;
					playerRankSettings.TextGravity = Gravity.East;
					using (var playerRankCaption = new MagickImage($"label:{arg[2]}", playerRankSettings))
					{
						background.Composite(playerRankCaption, width - margin - playerRankCaption.Width, rankHeight + index * 50, CompositeOperator.Over);
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

//lookup sender scoresaber in mongoDB via discord
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

//only 1 scoresaberID
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

//2 scoresabers
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