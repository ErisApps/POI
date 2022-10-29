using System.Runtime.Serialization;

namespace POI.ThirdParty.ScoreSaber.Models.Profile;

public enum SortType
{
	[EnumMember(Value = "recent")]
	Recent,

	[EnumMember(Value = "top")]
	Top
}