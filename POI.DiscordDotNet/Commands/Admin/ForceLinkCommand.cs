using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using POI.Core.Services;
using POI.DiscordDotNet.Commands.Beat_Saber;
using POI.DiscordDotNet.Services;

namespace POI.DiscordDotNet.Commands.Admin
{
	public class ForceLinkCommand : BaseLinkCommand
	{
		public ForceLinkCommand(ILogger<ForceLinkCommand> logger, ScoreSaberApiService scoreSaberApiService, MongoDbService mongoDbService, ScoreSaberLinkService scoreSaberLinkService)
			: base(logger, scoreSaberApiService, mongoDbService, scoreSaberLinkService)
		{
		}

		// ReSharper disable once StringLiteralTypo
		[Command("forcelink")]
		public async Task Handle(CommandContext ctx, [RemainingText] string _)
		{
			await IsProfileValid(ctx);
		}

		protected override void Test(CommandContext ctx, DiscordEmbedBuilder embedBuilder)
		{
			throw new System.NotImplementedException();
		}
	}
}