using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using POI.DiscordDotNet.Models.Database;
using POI.DiscordDotNet.Repositories;

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
			var serverDependentUserSettingsRepository = ctx.Services.GetRequiredService<ServerDependentUserSettingsRepository>();
			var serverDependentUserSettings = await serverDependentUserSettingsRepository.FindOneById(ctx.Member.Id, ctx.Guild.Id);
			return serverDependentUserSettings != null && serverDependentUserSettings.Permissions.HasFlag(_requiredPermission);
		}
	}
}