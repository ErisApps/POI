using BeatSaverSharp.Models;

namespace POI.ThirdParty.BeatSaver.Extensions;

public static class BeatmapExtensions
{
	public static BeatmapDifficulty.BeatmapCharacteristic MapToBeatmapCharacteristic(this string characteristicString)
	{
		return characteristicString switch
		{
			"Standard" => BeatmapDifficulty.BeatmapCharacteristic.Standard,
			"OneSaber" => BeatmapDifficulty.BeatmapCharacteristic.OneSaber,
			"NoArrows" => BeatmapDifficulty.BeatmapCharacteristic.NoArrows,
			"90Degree" => BeatmapDifficulty.BeatmapCharacteristic._90Degree,
			"360Degree" => BeatmapDifficulty.BeatmapCharacteristic._360Degree,
			"Lawless" => BeatmapDifficulty.BeatmapCharacteristic.Lawless,
			"Lightshow" => BeatmapDifficulty.BeatmapCharacteristic.Lightshow,
			_ => throw new NotImplementedException()
		};
	}

	public static BeatmapDifficulty.BeatSaverBeatmapDifficulty MapToBeatSaverBeatmapDifficulty(this string difficultyString)
	{
		return difficultyString switch
		{
			nameof(BeatmapDifficulty.BeatSaverBeatmapDifficulty.Easy) => BeatmapDifficulty.BeatSaverBeatmapDifficulty.Easy,
			nameof(BeatmapDifficulty.BeatSaverBeatmapDifficulty.Normal) => BeatmapDifficulty.BeatSaverBeatmapDifficulty.Normal,
			nameof(BeatmapDifficulty.BeatSaverBeatmapDifficulty.Hard) => BeatmapDifficulty.BeatSaverBeatmapDifficulty.Hard,
			nameof(BeatmapDifficulty.BeatSaverBeatmapDifficulty.Expert) => BeatmapDifficulty.BeatSaverBeatmapDifficulty.Expert,
			nameof(BeatmapDifficulty.BeatSaverBeatmapDifficulty.ExpertPlus) => BeatmapDifficulty.BeatSaverBeatmapDifficulty.ExpertPlus,
			_ => throw new NotImplementedException()
		};
	}
}