namespace POI.Persistence.Domain;

public class StarboardMessages
{
	public ulong ServerId { get; init; }

	public ulong ChannelId { get; init; }

	public ulong MessageId { get; init; }

	public ulong StarboardMessageId { get; init; }

	public StarboardMessages(ulong serverId, ulong channelId, ulong messageId, ulong starboardMessageId)
	{
		ServerId = serverId;
		ChannelId = channelId;
		MessageId = messageId;
		StarboardMessageId = starboardMessageId;
	}
}