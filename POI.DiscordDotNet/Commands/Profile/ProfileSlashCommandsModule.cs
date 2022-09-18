using DSharpPlus.SlashCommands;
using JetBrains.Annotations;

namespace POI.DiscordDotNet.Commands.Profile
{
	[SlashCommandGroup("profile", "Personal profile related commands"), UsedImplicitly]
	public partial class ProfileSlashCommandsModule : ApplicationCommandModule
	{
	}
}