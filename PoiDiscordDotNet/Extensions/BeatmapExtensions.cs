using System;
using System.Text.RegularExpressions;
using ImageMagick;

namespace PoiDiscordDotNet.Extensions
{
	internal static class BeatmapExtensions
	{
		private static readonly Regex ScoreSaberDifficultyRegex = new("_(?<difficulty>\\w+)_(?<characteristic>\\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

		internal static string MapToDifficulty(this int difficultyNumber)
		{
			return difficultyNumber switch
			{
				1 => "Easy",
				3 => "Normal",
				5 => "Hard",
				7 => "Expert",
				9 => "ExpertPlus",
				_ => throw new NotImplementedException()
			};
		}

		internal static MagickColor ReturnDifficultyColor(this int difficultyNumber)
		{
			return difficultyNumber switch
			{
				1 => Constants.DifficultyColors.Easy,
				3 => Constants.DifficultyColors.Normal,
				5 => Constants.DifficultyColors.Hard,
				7 => Constants.DifficultyColors.Expert,
				9 => Constants.DifficultyColors.ExpertPlus,
				_ => MagickColors.Gray
			};
		}

		internal static bool ParseScoreSaberDifficulty(this string difficultyRaw, out string? characteristic, out string? difficulty)
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

		internal static int NotesToMaxScore(this int valueNotes)
		{
			//TODO: Make it differently??
			var num1 = 0;
			int num2;
			for (num2 = 1; num2 < 8; num2 *= 2)
			{
				if (valueNotes >= num2 * 2)
				{
					num1 += (int) Math.Pow(num2, 2) * 2 + num2;
					valueNotes -= num2 * 2;
				}
				else
				{
					num1 += num2 * valueNotes;
					valueNotes = 0;
					break;
				}
			}

			return (num1 + valueNotes * num2) * 115;
		}
	}
}