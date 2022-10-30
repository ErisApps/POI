using System.Text.Json.Serialization;

namespace POI.ThirdParty.ScoreSaber.Models.Profile;

public readonly struct RefreshDto
{
	[JsonPropertyName("result")]
	public bool Result { get; }

	[JsonConstructor]
	public RefreshDto(bool result)
	{
		Result = result;
	}
}