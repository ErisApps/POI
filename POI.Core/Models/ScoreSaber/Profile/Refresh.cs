using System.Text.Json.Serialization;

namespace POI.Core.Models.ScoreSaber.Profile
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