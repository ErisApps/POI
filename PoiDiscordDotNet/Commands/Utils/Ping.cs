using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using PoiDiscordDotNet.Commands.Modules;

namespace PoiDiscordDotNet.Commands.Utils
{
    public class Ping : UtilCommandsModule
    {
	    [Command("ping")]
        public async Task PingCommand(CommandContext ctx)
        {
            await ctx.Channel
                .SendMessageAsync("POI!\n" +
                                  $"WS latency: {ctx.Client.Ping} ms\n" +
                                  $"Message latency: {(DateTimeOffset.Now - ctx.Message.Timestamp):g}")
                .ConfigureAwait(false);
        }
    }
}