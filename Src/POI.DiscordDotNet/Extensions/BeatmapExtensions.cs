using System;
using ImageMagick;
using POI.Core.Models.Shared;
using POI.DiscordDotNet.Services.Implementations;

namespace POI.DiscordDotNet.Extensions
{
	internal static class BeatmapExtensions
	{
		internal static string MapToDifficulty(this int difficultyNumber)
		{
			return difficultyNumber switch
			{
				(int) Difficulty.Easy => nameof(Difficulty.Easy),
				(int) Difficulty.Normal => nameof(Difficulty.Normal),
				(int) Difficulty.Hard => nameof(Difficulty.Hard),
				(int) Difficulty.Expert => nameof(Difficulty.Expert),
				(int) Difficulty.ExpertPlus => nameof(Difficulty.ExpertPlus),
				_ => throw new NotImplementedException()
			};
		}

		internal static MagickColor ReturnDifficultyColor(this Difficulty difficultyNumber)
		{
			return difficultyNumber switch
			{
				Difficulty.Easy => Constants.DifficultyColors.Easy,
				Difficulty.Normal => Constants.DifficultyColors.Normal,
				Difficulty.Hard => Constants.DifficultyColors.Hard,
				Difficulty.Expert => Constants.DifficultyColors.Expert,
				Difficulty.ExpertPlus => Constants.DifficultyColors.ExpertPlus,
				_ => MagickColors.Gray
			};
		}

		internal static uint NotesToMaxScore(this int valueNotes)
		{
			return NotesToMaxScore((uint) valueNotes);
		}

		internal static uint NotesToMaxScore(this uint valueNotes)
		{
			//TODO: Make it differently??
			uint num1 = 0;
			uint num2;
			for (num2 = 1; num2 < 8; num2 *= 2)
			{
				if (valueNotes >= num2 * 2)
				{
					num1 += (uint) Math.Pow(num2, 2) * 2 + num2;
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