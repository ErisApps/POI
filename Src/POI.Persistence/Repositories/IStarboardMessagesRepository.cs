using POI.Persistence.Domain;

namespace POI.Persistence.Repositories;

public interface IStarboardMessagesRepository
{
	Task<StarboardMessages?> FindOneByServerIdAndChannelIdAndMessageId(ulong serverId, ulong channelId, ulong messageId, CancellationToken cts = default);
	Task Insert(StarboardMessages entry, CancellationToken cts = default);
}