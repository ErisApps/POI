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
	public class RecentSongCommand : BaseSongCommand
	{
		private static readonly ActivitySource ActivitySource = new("POI.DiscordDotNet");

		public RecentSongCommand(ILogger<RecentSongCommand> logger, PathProvider pathProvider, IScoreSaberApiService scoreSaberApiService, IGlobalUserSettingsRepository globalUserSettingsRepository,
			IBeatSaverClientProvider beatSaverClientProvider, IBeatSaviorApiService beatSaviorApiService)
			: base(logger, scoreSaberApiService, globalUserSettingsRepository, beatSaverClientProvider, Path.Combine(pathProvider.AssetsPath, "poinext1.png"),
				Path.Combine(pathProvider.AssetsPath, "Signature-Eris.png"), beatSaviorApiService, ActivitySource)
		{
		}

		[Command("recentsong")]
		[Aliases("eris", "recentscore", "rs")]
		public async Task Handle(CommandContext ctx, [RemainingText] string _)
		{
			using var activity = ActivitySource.StartActivity(nameof(RecentSongCommand), kind: ActivityKind.Client);
			activity?.AddTag("discord.user.id", ctx.User.Id.ToString());
			activity?.AddTag("discord.user.name", ctx.User.Username);
			await GenerateScoreImageAndSendInternal(ctx);
		}

		protected override Task<PlayerScoresWrapperDto?> FetchScorePage(string playerId, uint page)
		{
			return ScoreSaberApiService.FetchRecentSongsScorePage(playerId, page);
		}
	}
}