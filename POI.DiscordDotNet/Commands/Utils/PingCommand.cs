﻿using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using POI.DiscordDotNet.Commands.Modules.SlashCommands;

namespace POI.DiscordDotNet.Commands.Utils
{
	public class PingCommand : UtilSlashCommandsModule
	{
		[SlashCommand("ping", "Shows how response I am ^^")]
		public async Task Handle(InteractionContext ctx)
		{
			await ctx
				.CreateResponseAsync("POI!\n" +
				                     $"WS latency: {ctx.Client.Ping} ms\n")
				.ConfigureAwait(false);
		}
	}
}