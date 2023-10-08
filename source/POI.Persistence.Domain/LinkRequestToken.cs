namespace POI.Persistence.Domain;

public class LinkRequestToken
{
	public required string LoginToken { get; set; }
	public required ulong DiscordId { get; set; }
	public DateTimeOffset CreatedAt { get; set; }
}