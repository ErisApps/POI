using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using JetBrains.Annotations;
using POI.DiscordDotNet.Commands.Modules.SlashCommands;

namespace POI.DiscordDotNet.Commands.Test;

[UsedImplicitly]
public class TestCommand : TestSlashCommandsModule
{
	[SlashCommand("test", "Just a generic command that can be used for testing stoofs 😅")]
	public async Task Handle(InteractionContext ctx)
	{
		var discordInteractionResponseBuilder = new DiscordInteractionResponseBuilder()
			.WithCustomId("modal-id")
			.WithTitle("Link your account")
			.AddComponents(new TextInputComponent("Test label", "customId", "Placeholder", required: true, style: TextInputStyle.Short))
			.AsEphemeral();

		await ctx.CreateResponseAsync(InteractionResponseType.Modal, discordInteractionResponseBuilder).ConfigureAwait(false);

		var interactivity = ctx.Client.GetInteractivity();
		var response = await interactivity.WaitForModalAsync("modal-id", ctx.User, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

		if (!response.TimedOut)
		{
			var inter = response.Result.Interaction;
			var interactionResponseBuilder = new DiscordInteractionResponseBuilder()
				.WithTitle("interaction response")
				.WithContent(response.Result.Values["customId"]);
			await inter.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, interactionResponseBuilder).ConfigureAwait(false);
		}
		// await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, discordInteractionResponseBuilder).ConfigureAwait(false);
	}
}