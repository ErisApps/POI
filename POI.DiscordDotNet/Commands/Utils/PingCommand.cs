using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using POI.DiscordDotNet.Commands.Modules;

namespace POI.DiscordDotNet.Commands.Utils
{
    public class PingCommand : UtilCommandsModule
    {
	    [Command("ping")]
        public async Task Handle(CommandContext ctx)
        {
            await ctx.Channel
                .SendMessageAsync("POI!\n" +
                                  $"WS latency: {ctx.Client.Ping} ms\n" +
                                  $"Message latency: {(DateTimeOffset.Now - ctx.Message.Timestamp):g}")
                .ConfigureAwait(false);
        }
    }
}