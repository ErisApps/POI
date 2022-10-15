using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;

namespace POI.DiscordDotNet.Extensions
{
	internal static class LoggerExtensions
	{
		internal static async Task LogError(this ILogger logger, CommandContext ctx, string? errorDetails = null, bool shouldDelete = true)
		{
			var errorMessage = await ctx.RespondAsync("I'm really sorry, but something went wrong. Please don't get mad at me, but ask try asking me again at a later time :(" +
			                                          $"{(string.IsNullOrWhiteSpace(errorDetails) ? string.Empty : $"\nDetails: {errorDetails}")}");

			logger.LogError("{ErrorMessage}", errorMessage);

			if (shouldDelete)
			{
				await Task.Delay(TimeSpan.FromSeconds(5));
				await errorMessage.DeleteAsync().ConfigureAwait(false);
			}
		}
	}
}