using DSharpPlus.SlashCommands;
using JetBrains.Annotations;
using NodaTime.Extensions;
using POI.DiscordDotNet.Services;

namespace POI.DiscordDotNet.Commands.SlashCommands.Utils
{
	[UsedImplicitly]
    public class UptimeCommand : UtilSlashCommandsModule
    {
	    private readonly IUptimeManagementService _uptimeManagementService;

        public UptimeCommand(IUptimeManagementService uptimeManagementService)
        {
	        _uptimeManagementService = uptimeManagementService;
        }

        [SlashCommand("uppy", "Shows how long I've been online already 😅"), UsedImplicitly]
        public async Task Handle(InteractionContext ctx)
        {
	        string message;
            var upSince = _uptimeManagementService.UpSince;
            if (upSince != null)
            {
                var duration = DateTimeOffset.Now.ToInstant().Minus(upSince.Value);
                message = $"I've been online for... {duration.ToString()}";
            }
            else
            {
                message = "I've been online for... erm... some time I guess Tehe";
            }

            await ctx.CreateResponseAsync(message).ConfigureAwait(false);
        }
    }
}