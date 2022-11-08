using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using POI.Core.Services;
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
					sc.AddSingleton<Constants>();
					sc.AddSingleton<IConstants>(provider => provider.GetRequiredService<Constants>());
					sc.AddScoreSaber();
				})
				.Build();

			host.Run();
		}
	}
}