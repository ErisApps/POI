namespace POI.ThirdParty.BeatSaver.Services;

public interface IBeatSaverClientProvider
{
	BeatSaverSharp.BeatSaver GetClientInstance();
}