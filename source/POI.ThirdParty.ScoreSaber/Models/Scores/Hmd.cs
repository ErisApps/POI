namespace POI.ThirdParty.ScoreSaber.Models.Scores;

/// <remark>
/// Based on https://github.com/ScoreSaber/ScoreSaber-Frontend/blob/main/src/lib/utils/helpers.ts#L85-L95
/// </remark>
[Flags]
// ReSharper disable once InconsistentNaming
public enum HMD
{
	Unknown = 0,
	// ReSharper disable once InconsistentNaming
	OculusRiftCV1 = 1,
	Vive = 2,
	VivePro = 4,
	WindowsMixedReality = 8,
	OculusRiftS = 16,
	OculusQuest = 32,
	ValveIndex = 64,
	ViveCosmos = 128
}