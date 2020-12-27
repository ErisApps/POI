using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;

namespace PoiDiscordDotNet.Commands.Utils
{
    public class Ping : BaseCommandModule
    {
        private readonly ILogger<Ping> _logger;

        public Ping(ILogger<Ping> logger)
        {
            _logger = logger;
        }

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