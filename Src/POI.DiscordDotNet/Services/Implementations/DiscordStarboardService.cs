using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using POI.Persistence.Domain;
using POI.Persistence.Repositories;

namespace POI.DiscordDotNet.Services.Implementations;

public class DiscordStarboardService : IAddDiscordClientFunctionality
{
	private readonly ILogger<DiscordChatCommandsService> _logger;
	private readonly IStarboardMessagesRepository _starboardMessagesRepository;
	private readonly IServerSettingsRepository _serverSettingsRepository;

	public DiscordStarboardService(
		ILogger<DiscordChatCommandsService> logger,
		IStarboardMessagesRepository starboardMessagesRepository, IServerSettingsRepository serverSettingsRepository)
	{
		_logger = logger;
		_starboardMessagesRepository = starboardMessagesRepository;
		_serverSettingsRepository = serverSettingsRepository;
	}

	public void Setup(IDiscordClientProvider discordClientProvider)
	{
		_logger.LogDebug("Setting up DiscordStarboardService");

		var client = discordClientProvider.Client!;
		client.MessageReactionAdded -= OnReactionAdded;
		client.MessageReactionAdded += OnReactionAdded;
	}

	public void Cleanup(IDiscordClientProvider discordClientProvider)
	{
		_logger.LogDebug("Cleaning up DiscordStarboardService");
		var client = discordClientProvider.Client!;
		client.MessageReactionAdded -= OnReactionAdded;
	}

	private async Task OnReactionAdded(DiscordClient sender, MessageReactionAddEventArgs args)
	{
		//TODO: Check if message was not cached
		//TODO: Check on emoji ID, not name
		//TODO: Figure out a way to get the display name of the user outside the guild...

		var serverSettings = await _serverSettingsRepository.FindOneById(args.Channel.Guild.Id);
		if (serverSettings?.StarboardChannelId == null)
		{
			_logger.LogError("Server settings or starboard channel id not found!");
			return;
		}

		// Skip event if the message is in the starboard channel (To prevent people staring the bot messages)
		if(args.Channel.Id == serverSettings.StarboardChannelId)
		{
			return;
		}

		// Check if the message reactions contains the star emote
		if (args.Message.Reactions.All(x => x.Emoji.Name != "⭐"))
		{
			return;
		}

		var messageStarCount = args.Message.Reactions.First(x => x.Emoji.Name == "⭐").Count;
		// Check if the message has enough stars
		if (messageStarCount < serverSettings.StarboardEmojiCount)
		{
			return;
		}

		// Get the starboard channel by the server settings id
		var starboardChannel = await sender.GetChannelAsync(serverSettings.StarboardChannelId.Value);
		if (starboardChannel == null)
		{
			_logger.LogError("Starboard channel not found!");
			return;
		}

		// Check if the message is cached and get contents if true.
		var message = args.Message;
		if (args.Message.Author == null)
		{
			message = await args.Channel.GetMessageAsync(args.Message.Id, true);
		}

		var embed = GetStarboardEmbed(message.Author.Username, args.Message.Channel.Name, message.Content, message.JumpLink, message.Timestamp, (uint)messageStarCount, message.Attachments.FirstOrDefault()?.Url);

		// Get the starboard message from the database
		var foundMessage = await _starboardMessagesRepository.FindOneByServerIdAndChannelIdAndMessageId(args.Guild.Id, args.Channel.Id, args.Message.Id);
		// If the message is already in the database, update the star count
		// (This will also update the message contents)
		if(foundMessage != null)
		{
			var starboardMessage = await starboardChannel.GetMessageAsync(foundMessage.StarboardMessageId);
			await starboardMessage.ModifyAsync(msg => msg.Embed = embed);
			_logger.LogInformation("Updated message {JumpLink} with {stars} stars!", args.Message.JumpLink, messageStarCount);
			return;
		}

		// If the message is not in the database, create a new starboard message
		var embedMessage = await starboardChannel.SendMessageAsync(embed);
		// And add to the database.
		await _starboardMessagesRepository.Insert(new StarboardMessages(args.Guild.Id, args.Channel.Id, args.Message.Id, embedMessage.Id));
		_logger.LogInformation("Message {JumpLink} sent to starboard channel!", args.Message.JumpLink);
	}

	private static DiscordEmbed GetStarboardEmbed(string userName, string channelName, string content, Uri jumpLink, DateTimeOffset timestamp, uint messageStarCount, string? attachmentUrl = null)
	{
		var builder = new DiscordEmbedBuilder()
			.WithTitle($"{userName} in #{channelName}")
			.WithDescription(content)
			// .WithDescription(content.Length > 50 ? content[..50] + "..." : content)
			.WithColor(new DiscordColor(0x6CCAF1))
			.WithUrl(jumpLink)
			.WithFooter($"⭐{messageStarCount}")
			.WithTimestamp(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(timestamp, "Europe/Brussels"));

		if (!string.IsNullOrWhiteSpace(attachmentUrl))
		{
			builder.WithImageUrl(attachmentUrl);
		}

		return builder.Build();
	}
}