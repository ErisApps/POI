using POI.ThirdParty.ScoreSaber.Models.Scores;

namespace POI.ThirdParty.ScoreSaber.Extensions;

public static class HmdExtensions
{
	public static bool HasFlagFast(this HMD value, HMD flag)
	{
		return (value & flag) != 0;
	}
}