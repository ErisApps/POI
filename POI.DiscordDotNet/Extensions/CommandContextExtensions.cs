using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;

namespace POI.DiscordDotNet.Extensions
{
	internal static class CommandContextExtensions
	{
		internal static async Task Errored(this CommandContext ctx, string? errorDetails = null, bool shouldDelete = true)
		{
			var errorMessage = await ctx.RespondAsync("I'm really sorry, but something went wrong. Please don't get mad at me, but ask try asking me again at a later time :(" +
			                        $"{(string.IsNullOrWhiteSpace(errorDetails) ? string.Empty : $"\nDetails: {errorDetails}")}");
			if (shouldDelete)
			{
				await Task.Delay(TimeSpan.FromSeconds(5));
				await errorMessage.DeleteAsync().ConfigureAwait(false);
			}
		}
	}
}