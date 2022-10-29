using Microsoft.Extensions.DependencyInjection;
using POI.Core.Services;
using POI.ThirdParty.ScoreSaber.Services;
using POI.ThirdParty.ScoreSaber.Services.Implementations;

namespace POI.ThirdParty.ScoreSaber.Extensions;

public static class ServiceCollectionExtensions
{
	///  <summary>
	///		Registers the services of the Core project
	///  </summary>
	///  <param name="serviceCollection">The container in which the services of the project have to be registered</param>
	///  <remark>
	///		Make sure that the <see cref="IConstantsCore">IConstants</see> and <see cref="Microsoft.Extensions.Logging.ILogger">ILogger</see> interfaces has been
	///		registered before calling this method
	///  </remark>
	public static IServiceCollection AddScoreSaber(this IServiceCollection serviceCollection)
	{
		serviceCollection.AddSingleton<IScoreSaberApiService, ScoreSaberApiService>();

		return serviceCollection;
	}
}