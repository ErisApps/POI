using DSharpPlus.SlashCommands;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using POI.DiscordDotNet.Commands.SlashCommands.Test;

namespace POI.DiscordDotNet.Commands.SlashCommands.Modules
{
	[SlashCommandGroup("test", "Test commands"), UsedImplicitly]
	public class TestSlashCommandsModule : ApplicationCommandModule
	{
		[SlashCommand("pacman", "Just a generic command that can be used for testing ImageSharp stoofs 😅"), UsedImplicitly]
		public Task HandlePacmanCommand(InteractionContext ctx,
			[Option("bgColor", "Color of the background (hex)")]
			string bgColorRaw,
			[Option("pacmanColor", "Color of the pacman body (hex)")]
			string pacmanColorRaw,
			[Option("pacmanEyeColor", "Color of the eye gif (hex)")]
			string pacmanEyeColorRaw)
		{
			return ctx.Services.GetRequiredService<PacmanCommand>().Handle(ctx, bgColorRaw, pacmanColorRaw, pacmanEyeColorRaw);
		}
	}
}