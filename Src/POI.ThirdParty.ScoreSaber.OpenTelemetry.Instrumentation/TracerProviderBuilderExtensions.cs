using OpenTelemetry.Trace;

namespace POI.ThirdParty.ScoreSaber.OpenTelemetry.Instrumentation;

public static class TracerProviderBuilderExtensions
{
	public static TracerProviderBuilder AddScoreSaberInstrumentation(this TracerProviderBuilder builder)
	{
		if (builder == null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		return builder.AddSource("POI.ThirdParty.ScoreSaber.OpenTelemetry.Instrumentation");
		//builder.AddInstrumentation(() => new ScoreSaberInstrumentation());
	}
}