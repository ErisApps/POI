using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using POI.Core.Services;

namespace POI.Core.Extensions
{
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Registers the services of the Core project
		/// </summary>
		/// <param name="serviceCollection">The container in which the services of the project have to be registered</param>
		/// <param name="name">The name of the host project (mainly used for user-agents)</param>
		/// <param name="version">The version of the host project (mainly used for user-agents)</param>
		/// <remark>
		///	Make sure that the ILogger interface has been registered before calling this method
		/// </remark>
		public static IServiceCollection AddCoreServices(this IServiceCollection serviceCollection, string name, Version version)
		{
			serviceCollection
				.AddSingleton(_ => new BeatSaverClientProvider(name, version))
				.AddSingleton(sc => new ScoreSaberService(sc.GetService<ILogger<ScoreSaberService>>()!, name, version));

			return serviceCollection;
		}
	}
}