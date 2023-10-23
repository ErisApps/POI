using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using POI.DiscordDotNet.Commands.SlashCommands.Modules;
using POI.Persistence.Repositories;

namespace POI.DiscordDotNet.Commands.SlashCommands.Utils;

public class ModMailSlashCommands : UtilSlashCommandsModule
{
	private readonly IServerSettingsRepository _serverSettingsRepository;
	private readonly ILogger<ModMailSlashCommands> _logger
		;

	public ModMailSlashCommands(IServerSettingsRepository serverSettingsRepository, ILogger<ModMailSlashCommands> logger)
	{
		_serverSettingsRepository = serverSettingsRepository;
		_logger = logger;
	}

	public async Task Handle(InteractionContext ctx, string message, bool anonymously)
	{
		await ctx.CreateResponseAsync("Message has been sent.", true).ConfigureAwait(false);
		var serverId = ctx.Guild.Id;
		var serverSettings = await _serverSettingsRepository.FindOneById(serverId);
		if (serverSettings?.ModMailChannelId == null)
		{
			_logger.LogWarning("Server settings or ModMailChannelId for {ServerId} not found", serverId);
			return;
		}

		var channelId = serverSettings.ModMailChannelId!.Value;
		DiscordChannel channel;
		try{
			channel = await ctx.Client.GetChannelAsync(channelId);
		}
		catch (Exception e)
		{
			_logger.LogWarning(e, "Channel {ChannelId} not found for server {ServerId}",channelId, serverId);
			return;
		}
		var name = anonymously ? "Anonymous" : ctx.User.Username;
		var embed = new DiscordEmbedBuilder()
			.WithTitle($"New mod mail from {name}")
			.WithDescription(message)
			.WithTimestamp(DateTimeOffset.UtcNow)
			.Build();
		await channel.SendMessageAsync(embed).ConfigureAwait(false);
		_logger.LogInformation("Mod mail sent from {UserId} to {ChannelId} on {ServerId}", name, channelId, serverId);
	}
}