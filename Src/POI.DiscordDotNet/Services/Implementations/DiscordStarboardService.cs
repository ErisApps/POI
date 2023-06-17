using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace POI.DiscordDotNet.Services.Implementations;

public class DiscordStarboardService: IAddDiscordClientFunctionality
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<DiscordChatCommandsService> _logger;


	public DiscordStarboardService(
		IServiceProvider serviceProvider,
		ILogger<DiscordChatCommandsService> logger)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
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
		//TODO: replace with env variables
		const int starCount = 2;
		const long starBoardChannelId = 1014160450813968384;


		// Check if the message reactions contains the star emote
		if (args.Message.Reactions.All(x => x.Emoji.Name != "⭐") || args.Message.Reactions.First(x => x.Emoji.Name == "⭐").Count < starCount)
		{
			return;
		}

		// Send message in starboard channel
		var starboardChannel = await sender.GetChannelAsync(starBoardChannelId);
		if (starboardChannel == null)
		{
			_logger.LogError("Starboard channel not found!");
			return;
		}

		var title = args.Message.Content.Length > 50 ? args.Message.Content[..50] + "..." : args.Message.Content;
		var description = $"[Jump to message]({args.Message.JumpLink})";
		var footer = $"#{args.Message.Channel.Name}";
		var timestamp = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(args.Message.Timestamp, "Europe/Brussels");

		var embed = new DiscordEmbedBuilder()
			.WithTitle(title)
			.WithDescription(description)
			.WithColor(DiscordColor.Gold)
			.WithFooter(footer)
			.WithTimestamp(timestamp)
			.Build();

		await starboardChannel.SendMessageAsync(embed);

		_logger.LogInformation("Message {JumpLink} sent to starboard channel!", args.Message.JumpLink);
	}
}