using System.Runtime.Serialization;

namespace POI.Core.Models.ScoreSaber.New.Profile
{
	public enum SortType
	{
		[EnumMember(Value = "recent")]
		Recent,

		[EnumMember(Value = "top")]
		Top
	}
}