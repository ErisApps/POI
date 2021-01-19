using System.Text.RegularExpressions;

namespace PoiDiscordDotNet.Extensions
{
	public static class StringExtensions
	{
		private static readonly Regex ScoreSaberIdRegex = new Regex("(?:http(?:s)?://)?(?:new\\.)?(?:scoresaber\\.com/u/)?(\\d{16,})(?:/.*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		internal static bool ExtractScoreSaberId(this string input, out string? scoreSaberId)
		{
			var matchResult = ScoreSaberIdRegex.Match(input);
			scoreSaberId = matchResult.Success ? matchResult.Groups["scoreSaberId"].Value : null;
			return matchResult.Success;
		}
	}
}