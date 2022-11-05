using DSharpPlus.SlashCommands;
using JetBrains.Annotations;
using NodaTime.Text;
using POI.DiscordDotNet.Persistence.Repositories;

namespace POI.DiscordDotNet.Commands.Profile
{
	public partial class ProfileSlashCommandsModule
	{
		[SlashCommandGroup("birthday", "Commands related to managing your birthday"), UsedImplicitly]
		public class BirthdaySlashCommandsModule : ApplicationCommandModule
		{
			private readonly IGlobalUserSettingsRepository _globalUserSettingsRepository;

			private readonly LocalDatePattern _localDatePattern;

			public BirthdaySlashCommandsModule(IGlobalUserSettingsRepository globalUserSettingsRepository)
			{
				_globalUserSettingsRepository = globalUserSettingsRepository;

				_localDatePattern = LocalDatePattern.CreateWithInvariantCulture("dd'-'MM'-'uuuu");
			}

			[SlashCommand("set", "Sets your birthday"), UsedImplicitly]
			public async Task Set(InteractionContext ctx, [Option("date", "The date of your birthday. Format: dd-MM-yyyy Example: 31-10-1998")] string birthdayDateRaw)
			{
				var parseResult = _localDatePattern.Parse(birthdayDateRaw);
				if (parseResult.Success)
				{
					await _globalUserSettingsRepository.UpdateBirthday(ctx.User.Id, parseResult.Value).ConfigureAwait(false);
					await ctx.CreateResponseAsync("Birthday has been updated").ConfigureAwait(false);
				}
				else
				{
					await ctx.CreateResponseAsync("Couldn't parse the birthday date, please make sure that you used the correct format. Format: dd-MM-yyyy Example: 31-10-1998").ConfigureAwait(false);
				}
			}

			[SlashCommand("clear", "Unsets your birthday"), UsedImplicitly]
			public async Task Clear(InteractionContext ctx)
			{
				await _globalUserSettingsRepository.UpdateBirthday(ctx.User.Id, null).ConfigureAwait(false);

				await ctx.CreateResponseAsync("Birthday has been cleared").ConfigureAwait(false);
			}
		}
	}
}