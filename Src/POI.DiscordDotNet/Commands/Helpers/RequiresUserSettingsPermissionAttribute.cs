using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using POI.Persistence.Domain;
using POI.Persistence.Repositories;

namespace POI.DiscordDotNet.Commands.Helpers
{
	/// <remark>
	///		Stopgap command check until we fully move over to slash commands
	/// </remark>
	public class RequiresUserSettingsPermissionAttribute : CheckBaseAttribute
	{
		private readonly Permissions _requiredPermission;

		public RequiresUserSettingsPermissionAttribute(Permissions requiredPermission)
		{
			_requiredPermission = requiredPermission;
		}

		public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
		{
			if (ctx.Guild?.Id == null)
			{
				return false;
			}

			var serverDependentUserSettingsRepository = ctx.Services.GetRequiredService<IServerDependentUserSettingsRepository>();
			var serverDependentUserSettings = await serverDependentUserSettingsRepository.FindOneById(ctx.Member!.Id, ctx.Guild.Id);
			return serverDependentUserSettings != null && serverDependentUserSettings.Permissions.HasFlag(_requiredPermission);
		}
	}
}