using System.Text.RegularExpressions;

namespace POI.ThirdParty.ScoreSaber.Extensions;

public static class DifficultyInfoExtensions
{
	private static readonly Regex ScoreSaberDifficultyRegex = new("_(?<difficulty>\\w+)_(?<characteristic>\\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

	public static bool ParseScoreSaberDifficulty(this string difficultyRaw, out string? characteristic, out string? difficulty)
	{
		var matchResult = ScoreSaberDifficultyRegex.Match(difficultyRaw);
		if (matchResult.Success)
		{
			difficulty = matchResult.Groups["difficulty"].Value;
			characteristic = matchResult.Groups["characteristic"].Value.Replace("Solo", string.Empty);
		}
		else
		{
			difficulty = null;
			characteristic = null;
		}

		return matchResult.Success;
	}
}