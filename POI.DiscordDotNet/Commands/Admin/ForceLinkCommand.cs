using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using POI.Core.Services;
using POI.DiscordDotNet.Commands.BeatSaber;
using POI.DiscordDotNet.Services;

namespace POI.DiscordDotNet.Commands.Admin
{
	[RequireUserPermissions(Permissions.Administrator)]
	public class ForceLinkCommand : BaseLinkCommand
	{
		public ForceLinkCommand(ILogger<ForceLinkCommand> logger, ScoreSaberApiService scoreSaberApiService, ScoreSaberLinkService scoreSaberLinkService)
			: base(logger, scoreSaberApiService, scoreSaberLinkService)
		{
		}

		// ReSharper disable once StringLiteralTypo
		[Command("forcelink")]
		public async Task Handle(CommandContext ctx, [RemainingText] string _)
		{
			await IsProfileValid(ctx);
		}

		protected override DiscordEmbedBuilder EnrichProfileEmbedBuilderShared(DiscordEmbedBuilder embedBuilder)
		{
			throw new System.NotImplementedException();
		}
	}
}