using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
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

		[SlashCommand("silentmessage", "Send a message anonymously into this channel."), UsedImplicitly]
		[SlashCooldown(1, 60 * 5, SlashCooldownBucketType.Channel)] // TODO: extract to db + migrations
		public Task HandleSilentMessageCommand(InteractionContext ctx,
			[Option("Type", "What type of message do you want to send into this channel?")]
			AnonymousMessages messageType = AnonymousMessages.StayOnTopicReminder)
			=> ctx.Services.GetRequiredService<SilentMessageSlashCommands>().Handle(ctx, messageType);
	}
}