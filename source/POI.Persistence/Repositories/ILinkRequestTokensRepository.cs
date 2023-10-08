namespace POI.Persistence.Repositories;

public interface ILinkRequestTokensRepository
{
	Task<ulong> GetDiscordIdByToken(string token);
	Task<string> CreateToken(ulong discordUserId);
}