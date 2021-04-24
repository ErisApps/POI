using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using POI.Core.Extensions;
using POI.Core.Services.Interfaces;

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
					sc.AddSingleton<IConstantsCore, Constants>();
					sc.AddCoreServices();
				})
				.Build();

			host.Run();
		}
	}
}