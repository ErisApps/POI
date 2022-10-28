using System.Text.Json.Serialization;

namespace POI.ThirdParty.ScoreSaber.Models.Profile
{
	public readonly struct Refresh
	{
		[JsonPropertyName("result")]
		public bool Result { get; }

		[JsonConstructor]
		public Refresh(bool result)
		{
			Result = result;
		}
	}
}