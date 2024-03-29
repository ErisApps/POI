using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using JetBrains.Annotations;
using POI.DiscordDotNet.Commands.ChatCommands.Modules;

namespace POI.DiscordDotNet.Commands.ChatCommands.Admin
{
	[UsedImplicitly]
	[RequireUserPermissions(Permissions.Administrator)]
	public class PruneCommand : AdminCommandsModule
	{
		[Command("prune")]
		public async Task Handle(CommandContext ctx, int messagesPruneCount)
		{
			await ctx.Message.DeleteAsync().ConfigureAwait(false);

			var messagesToDelete = await ctx.Channel.GetMessagesAsync(messagesPruneCount).ConfigureAwait(false);
			await ctx.Channel.DeleteMessagesAsync(messagesToDelete, "Requested by prune command").ConfigureAwait(false);
			var deletionMessage = await ctx.RespondAsync($"I've successfully deleted {messagesPruneCount} message{(messagesPruneCount != 1 ? "s" : string.Empty)}.").ConfigureAwait(false);

			await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
			await deletionMessage.DeleteAsync().ConfigureAwait(false);
		}
	}
}