using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using NodaTime.Text;
using POI.DiscordDotNet.Services;

namespace POI.DiscordDotNet.Commands.Profile
{
	public partial class ProfileSlashCommandsModule
	{
		[SlashCommandGroup("birthday", "Commands related to your birthday")]
		public class BirthdaySlashCommandsModule : ApplicationCommandModule
		{
			private readonly UserSettingsService _userSettingsService;

			private readonly LocalDatePattern _localDatePattern;

			public BirthdaySlashCommandsModule(UserSettingsService userSettingsService)
			{
				_userSettingsService = userSettingsService;

				_localDatePattern = LocalDatePattern.CreateWithInvariantCulture("dd'-'MM'-'uuuu");
			}

			[SlashCommand("set", "Sets your birthday")]
			public async Task Set(InteractionContext ctx, [Option("date", "The date of your birthday. Format: dd-MM-yyyy Example: 31-10-1998")] string birthdayDateRaw)
			{
				var parseResult = _localDatePattern.Parse(birthdayDateRaw);
				if (parseResult.Success)
				{
					await _userSettingsService.UpdateBirthday(ctx.User.Id.ToString(), parseResult.Value).ConfigureAwait(false);
					await ctx.CreateResponseAsync("Birthday has been updated").ConfigureAwait(false);
				}
				else
				{
					await ctx.CreateResponseAsync("Couldn't parse the birthday date, please make sure that you used the correct format. Format: dd-MM-yyyy Example: 31-10-1998").ConfigureAwait(false);
				}
			}

			[SlashCommand("clear", "Unsets your birthday")]
			public async Task Clear(InteractionContext ctx)
			{
				await _userSettingsService.UpdateBirthday(ctx.User.Id.ToString(), null).ConfigureAwait(false);

				await ctx.CreateResponseAsync("Birthday has been cleared").ConfigureAwait(false);
			}
		}
	}
}