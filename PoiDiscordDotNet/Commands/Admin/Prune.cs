using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;

namespace PoiDiscordDotNet.Commands.Admin
{
	[RequireUserPermissions(Permissions.Administrator)]
	public class Prune : BaseCommandModule
	{
		private readonly ILogger<Prune> _logger;

		public Prune(ILogger<Prune> logger)
		{
			_logger = logger;
		}

		[Command("prune")]
		public async Task PruneCommand(CommandContext ctx, int messagesPruneCount)
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