using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace POI.Azure.Functions
{
	public static class ExampleTimerFunction
	{
		[Function(nameof(ExampleTimerFunction))]
		public static Task RunAsync([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer, FunctionContext executionContext)
		{
			var logger = executionContext.GetLogger(nameof(ExampleTimerFunction));
			logger.LogInformation("C# Timer trigger function executed at: {Time}", DateTime.UtcNow);

			return Task.CompletedTask;
		}
	}
}