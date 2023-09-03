using DSharpPlus.SlashCommands;
using POI.DiscordDotNet.Commands.SlashCommands.Modules;

namespace POI.DiscordDotNet.Commands.SlashCommands.Utils;

public enum AnonymousMessages
{
	[ChoiceName("Stay on Topic reminder")] StayOnTopicReminder
}

public class SilentMessageSlashCommands: UtilSlashCommandsModule
{
	private readonly Dictionary<AnonymousMessages, string> _anonymousMessages = new()
	{
		{ AnonymousMessages.StayOnTopicReminder, "\u26a0\ufe0f Beep boop, please stay on topic! \ud83d\ude4f" }
	};

	public async Task Handle(InteractionContext ctx,
		AnonymousMessages messageType = AnonymousMessages.StayOnTopicReminder)
	{
		await ctx.CreateResponseAsync("Message has been sent.", true).ConfigureAwait(false);
		await ctx.Channel.SendMessageAsync(_anonymousMessages[messageType]).ConfigureAwait(false);
	}
}