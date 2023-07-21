using DSharpPlus.Entities;

namespace POI.DiscordDotNet.Extensions
{
	internal static class EmbedBuilderExtensions
	{
		internal static DiscordEmbedBuilder WithPoiColor(this DiscordEmbedBuilder discordEmbedBuilder)
		{
			discordEmbedBuilder.WithColor(3447003);

			return discordEmbedBuilder;
		}
	}
}