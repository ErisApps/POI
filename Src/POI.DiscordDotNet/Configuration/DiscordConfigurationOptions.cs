namespace POI.DiscordDotNet.Configuration;

public class DiscordConfigurationOptions
{
	public const string SECTION_NAME = "Discord";

	public string Token { get; init; }
	public string Prefix { get; init; }
}