using Microsoft.Extensions.DependencyInjection;
using POI.Core.Services;

namespace POI.Core.Extensions
{
	public static class ServiceCollectionExtensions
	{
		///  <summary>
		///		Registers the services of the Core project
		///  </summary>
		///  <param name="serviceCollection">The container in which the services of the project have to be registered</param>
		///  <remark>
		///		Make sure that the <see cref="POI.Core.Services.Interfaces.IConstantsCore">IConstants</see> and <see cref="Microsoft.Extensions.Logging.ILogger">ILogger</see> interfaces has been
		///		registered before calling this method
		///  </remark>
		public static IServiceCollection AddCoreServices(this IServiceCollection serviceCollection)
		{
			serviceCollection
				.AddSingleton<BeatSaverClientProvider>()
				.AddSingleton<ScoreSaberApiService>()
				.AddSingleton<ScoreSaberScraperService>();

			return serviceCollection;
		}
	}
}