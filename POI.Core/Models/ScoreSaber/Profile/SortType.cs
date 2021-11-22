using System.Runtime.Serialization;

namespace POI.Core.Models.ScoreSaber.Profile
{
	public enum SortType
	{
		[EnumMember(Value = "recent")]
		Recent,

		[EnumMember(Value = "top")]
		Top
	}
}