using System.Diagnostics;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using POI.Persistence.Repositories;
using POI.DiscordDotNet.Services.Implementations;
using POI.ThirdParty.BeatSaver.Services;
using POI.ThirdParty.BeatSavior.Services;
using POI.ThirdParty.ScoreSaber.Models.Wrappers;
using POI.ThirdParty.ScoreSaber.Services;

namespace POI.DiscordDotNet.Commands.BeatSaber
{
	[UsedImplicitly]
	public class TopSongCommand : BaseSongCommand
	{
		private static readonly ActivitySource ActivitySource = new("POI.DiscordDotNet");

		public TopSongCommand(ILogger<TopSongCommand> logger, PathProvider pathProvider, IScoreSaberApiService scoreSaberApiService, IGlobalUserSettingsRepository globalUserSettingsRepository,
			IBeatSaverClientProvider beatSaverClientProvider, IBeatSaviorApiService beatSaviorApiService)
			: base(logger, scoreSaberApiService, globalUserSettingsRepository, beatSaverClientProvider, Path.Combine(pathProvider.AssetsPath, "poinext1.png"),
				Path.Combine(pathProvider.AssetsPath, "Signature-Eris.png"), beatSaviorApiService, ActivitySource)
		{
		}

		[Command("topsong")]
		[Aliases("topscore", "ts")]
		public async Task Handle(CommandContext ctx, [RemainingText] string _)
		{
			using var activity = ActivitySource.StartActivity(nameof(TopSongCommand), kind: ActivityKind.Client);
			activity?.AddTag("discord.user.id", ctx.User.Id.ToString());
			activity?.AddTag("discord.user.name", ctx.User.Username);
			await GenerateScoreImageAndSendInternal(ctx).ConfigureAwait(false);
		}

		protected override Task<PlayerScoresWrapperDto?> FetchScorePage(string playerId, uint page)
		{
			return ScoreSaberApiService.FetchTopSongsScorePage(playerId, page);
		}
	}
}