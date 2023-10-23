using DSharpPlus.SlashCommands;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using POI.DiscordDotNet.Commands.SlashCommands.Utils;

namespace POI.DiscordDotNet.Commands.SlashCommands.Modules
{
	[SlashCommandGroup("utils", "A set of utility commands"), UsedImplicitly]
	public class UtilSlashCommandsModule : ApplicationCommandModule
	{
		[SlashCommand("ping", "Shows how responsive I am ^^"), UsedImplicitly]
		public async Task HandlePingCommand(InteractionContext ctx)
		{
			await ctx
				.CreateResponseAsync("POI!\n" +
				                     $"WS latency: {ctx.Client.Ping} ms\n")
				.ConfigureAwait(false);
		}

		[SlashCommand("uppy", "Shows how long I've been online already 😅"), UsedImplicitly]
		public Task HandleUptimeCommand(InteractionContext ctx)
			=> ctx.Services.GetRequiredService<UptimeCommand>().Handle(ctx);

		[SlashCommand("modmail", "Send a message to the mods."), UsedImplicitly]
		public Task HandleSilentMessageCommand(InteractionContext ctx, [Option("Message", "Your message for the moderators.", true)] string message)
			=> ctx.Services.GetRequiredService<ModMailSlashCommands>().Handle(ctx, message);
	}
}