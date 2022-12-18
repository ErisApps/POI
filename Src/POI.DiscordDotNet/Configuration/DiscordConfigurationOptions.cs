namespace POI.DiscordDotNet.Configuration;

public class DiscordConfigurationOptions
{
	public const string SECTION_NAME = "Discord";

	public required string Token { get; init; }
	public required string Prefix { get; init; }
}