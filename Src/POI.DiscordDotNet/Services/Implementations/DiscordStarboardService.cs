using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using POI.Persistence.Domain;
using POI.Persistence.Repositories;

namespace POI.DiscordDotNet.Services.Implementations;

public class DiscordStarboardService : IAddDiscordClientFunctionality
{
	private readonly ILogger<DiscordStarboardService> _logger;
	private readonly IStarboardMessageRepository _starboardMessageRepository;
	private readonly IServerSettingsRepository _serverSettingsRepository;

	public DiscordStarboardService(
		ILogger<DiscordStarboardService> logger,
		IStarboardMessageRepository starboardMessageRepository, IServerSettingsRepository serverSettingsRepository)
	{
		_logger = logger;
		_starboardMessageRepository = starboardMessageRepository;
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
		//TODO: Check on emoji ID, not name
		//TODO: Figure out a way to get the display name of the user outside the guild...

		var guild = args.Guild;
		var serverSettings = await _serverSettingsRepository.FindOneById(guild.Id);

		if (serverSettings?.StarboardChannelId == null)
		{
			_logger.LogWarning("Server settings or starboard channel id not found for guild id {GuildId}!", guild.Id);
			return;
		}

		var channel = args.Channel;

		// Skip event if the message is in the starboard channel (To prevent people staring the bot messages)
		if (channel.Id == serverSettings.StarboardChannelId)
		{
			return;
		}

		var message = args.Message;

		// Check if the message reactions contains the star emote
		if (message.Reactions.All(x => x.Emoji.Name != "⭐"))
		{
			return;
		}

		var messageStarCount = message.Reactions.First(x => x.Emoji.Name == "⭐").Count;

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
		if (message.Author == null)
		{
			message = await channel.GetMessageAsync(message.Id, true);
		}

		var embed = GetStarboardEmbed(message.Author.Username, message.Channel.Name, message.Content, message.JumpLink, message.Timestamp, (uint) messageStarCount,
			message.Attachments.FirstOrDefault()?.Url);

		// Get the starboard message from the database
		var foundMessage = await _starboardMessageRepository.FindOneByServerIdAndChannelIdAndMessageId(guild.Id, channel.Id, message.Id);

		// If the message is not in the database, create a new starboard message
		if (foundMessage == null)
		{
			var embedMessage = await starboardChannel.SendMessageAsync(embed);
			// And add to the database.
			await _starboardMessageRepository.Insert(new StarboardMessages(guild.Id, channel.Id, message.Id, embedMessage.Id));
			_logger.LogInformation("Message {JumpLink} sent to starboard channel!", message.JumpLink);
		}
		else
		{
			// Else the message is already in the database, update the star count
			// (This will also update the message contents)
			var starboardMessage = await starboardChannel.GetMessageAsync(foundMessage.StarboardMessageId);
			await starboardMessage.ModifyAsync(msg => msg.Embed = embed);
			_logger.LogInformation("Updated message {JumpLink} with {Stars} stars!", message.JumpLink, messageStarCount);
		}
	}

	private static DiscordEmbed GetStarboardEmbed(string userName, string channelName, string content, Uri jumpLink, DateTimeOffset timestamp, uint messageStarCount, string? attachmentUrl = null)
	{
		var builder = new DiscordEmbedBuilder()
			.WithTitle($"{userName} in #{channelName}")
			.WithDescription(content)
			.WithColor(new DiscordColor(0x6CCAF1))
			.WithUrl(jumpLink)
			.WithFooter($"⭐{messageStarCount}")
			.WithTimestamp(timestamp);

		if (!string.IsNullOrWhiteSpace(attachmentUrl))
		{
			builder.WithImageUrl(attachmentUrl);
		}

		return builder.Build();
	}
}