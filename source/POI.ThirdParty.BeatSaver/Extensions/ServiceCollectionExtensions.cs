﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using POI.ThirdParty.BeatSaver.Services;
using POI.ThirdParty.BeatSaver.Services.Implementations;

namespace POI.ThirdParty.BeatSaver.Extensions;

public static class ServiceCollectionExtensions
{
	///  <summary>
	///		Registers the services of the POI.ThirdParty.BeatSaver project
	///  </summary>
	///  <param name="serviceCollection">The container in which the services of the project have to be registered</param>
	///  <remark>
	///		Make sure that the <see cref="POI.ThirdParty.Core.Services.IConstants">IConstants</see> interface has been registered before calling this method
	///  </remark>
	public static IServiceCollection AddBeatSaver(this IServiceCollection serviceCollection)
	{
		serviceCollection.TryAddSingleton<IBeatSaverClientProvider, BeatSaverClientProvider>();

		return serviceCollection;
	}
}