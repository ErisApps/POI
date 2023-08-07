using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using POI.Persistence.Repositories;

namespace POI.DiscordDotNet.Services.Implementations;

public class DiscordPinFirstEventThreadMessageService : IAddDiscordClientFunctionality
{
	private readonly IServerSettingsRepository _repository;
	private readonly ILogger<DiscordPinFirstEventThreadMessageService> _logger;

	public DiscordPinFirstEventThreadMessageService(IServerSettingsRepository repository, ILogger<DiscordPinFirstEventThreadMessageService> logger)
	{
		_repository = repository;
		_logger = logger;
	}

	public void Setup(IDiscordClientProvider discordClientProvider)
	{
		discordClientProvider.Client!.MessageCreated += OnMessageCreated;
	}

	public void Cleanup(IDiscordClientProvider discordClientProvider)
	{
		discordClientProvider.Client!.MessageCreated -= OnMessageCreated;
	}

	private async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs args)
	{
		var channel = args.Channel;
		if (channel.IsThread)
		{
			var guild = args.Guild;
			var serverSettings = await _repository.FindOneById(guild.Id);
			if (serverSettings?.EventsChannelId == null)
			{
				_logger.LogWarning("Server settings or event channel id not found for guild id {GuildId}!", guild.Id);
				return;
			}

			var eventsChannelId = serverSettings.EventsChannelId;
			if (channel.ParentId == eventsChannelId && guild.Threads.TryGetValue(channel.Id, out var thread) && (await thread.GetPinnedMessagesAsync()).Count == 0)
			{
				var creator = thread.CreatorId;
				var message = args.Message;
				if (args.Author.Id == creator && message.MessageType != (MessageType) 21)
				{
					await message.PinAsync();
				}
			}
		}
	}
}