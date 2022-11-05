namespace POI.DiscordDotNet.Configuration;

public class PathConfigurationOptions
{
	public const string SECTION_NAME = "Paths";

	public string DataFolderPath { get; init; }

	public string? AssetsFolderPath { get; init; }
	public string? LogsFolderPath { get; init; }
}