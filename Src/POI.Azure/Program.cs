using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using POI.ThirdParty.Core.Services;
using POI.ThirdParty.ScoreSaber.Extensions;

namespace POI.Azure
{
	public class Program
	{
		public static void Main()
		{
			var host = new HostBuilder()
				.ConfigureFunctionsWorkerDefaults()
				.ConfigureServices(sc =>
				{
					sc.AddSingleton<IConstants, Constants>();
					sc.AddScoreSaber();
				})
				.Build();

			host.Run();
		}
	}
}