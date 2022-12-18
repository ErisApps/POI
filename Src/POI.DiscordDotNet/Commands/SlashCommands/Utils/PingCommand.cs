using DSharpPlus.SlashCommands;
using JetBrains.Annotations;

namespace POI.DiscordDotNet.Commands.SlashCommands.Utils
{
	[UsedImplicitly]
	public class PingCommand : UtilSlashCommandsModule
	{
		[SlashCommand("ping", "Shows how responsive I am ^^"), UsedImplicitly]
		public async Task Handle(InteractionContext ctx)
		{
			await ctx
				.CreateResponseAsync("POI!\n" +
				                     $"WS latency: {ctx.Client.Ping} ms\n")
				.ConfigureAwait(false);
		}
	}
}