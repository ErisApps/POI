using Microsoft.Extensions.DependencyInjection;
using POI.Core.Services;
using POI.ThirdParty.BeatSaver.Services;
using POI.ThirdParty.BeatSaver.Services.Implementations;

namespace POI.ThirdParty.BeatSaver.Extensions
{
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
		public static IServiceCollection AddBeatSaver(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddSingleton<IBeatSaverClientProvider, BeatSaverClientProvider>();

			return serviceCollection;
		}
	}
}