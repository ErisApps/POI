using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using POI.ThirdParty.BeatSavior.Services;
using POI.ThirdParty.BeatSavior.Services.Implementations;

namespace POI.ThirdParty.BeatSavior.Extensions;

public static class ServiceCollectionExtensions
{
	///  <summary>
	///		Registers the services of the POI.ThirdParty.BeatSavior project
	///  </summary>
	///  <param name="serviceCollection">The container in which the services of the project have to be registered</param>
	///  <remark>
	///		Make sure that the <see cref="POI.Core.Services.IConstants">IConstants</see> and <see cref="Microsoft.Extensions.Logging.ILogger">ILogger</see> interfaces have been
	///		registered before calling this method
	///  </remark>
	public static IServiceCollection AddBeatSavior(this IServiceCollection serviceCollection)
	{
		serviceCollection.TryAddSingleton<IBeatSaviorApiService, BeatSaviorApiService>();

		return serviceCollection;
	}
}