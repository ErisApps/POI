using DSharpPlus.SlashCommands;
using JetBrains.Annotations;
using NodaTime.Text;

namespace POI.DiscordDotNet.Commands.SlashCommands.Utils
{
	public enum AnonymousMessages
	{
    	[ChoiceName("Stay on Topic reminder")]
    	StayOnTopicReminder = "Beep boop, stay on topic please!",
   	}

	public class SilentMessageSlashCommands
	{
		[SlashCommand("Send", "Send anonymous message to channel via POI"), UsedImplicitly]
		public async Task Set(InteractionContext ctx,  
			[ChannelTypes("text")] 
			[Option("Channel", "The channel where you want to send the message to")] DiscordChannel channel,
			[Option("Type", "What message do you want to send")] AnonymousMessages messageType = AnonymousMessages.StayOnTopicReminder)
		{
			await ctx.CreateResponseAsync("Message has been sent", new { IsEphemeral: true }).ConfigureAwait(false);
			var embedMessage = await channel.SendMessageAsync(messageType);
		}
	}
}