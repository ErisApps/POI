using Microsoft.Extensions.Hosting;
using POI.Core.Extensions;

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
					sc.AddCoreServices("POI Online", typeof(Program).Assembly.GetName().Version!);
				})
				.Build();

			host.Run();
		}
	}
}