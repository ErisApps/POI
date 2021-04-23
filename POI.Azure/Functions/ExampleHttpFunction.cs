using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace POI.Azure.Functions
{
	public static class ExampleHttpFunction
	{
		[Function(nameof(ExampleHttpFunction))]
		public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req,
			FunctionContext executionContext)
		{
			var logger = executionContext.GetLogger(nameof(ExampleHttpFunction));
			logger.LogInformation("message logged");

			var response = req.CreateResponse(HttpStatusCode.OK);
			response.Headers.Add("Date", "Mon, 18 Jul 2016 16:06:00 GMT");
			response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

			response.WriteString("Welcome to .NET 5!!");

			return response;
		}
	}
}