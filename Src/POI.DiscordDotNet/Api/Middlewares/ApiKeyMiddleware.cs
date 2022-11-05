using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using POI.DiscordDotNet.Services.Implementations;

namespace POI.DiscordDotNet.Api.Middlewares
{
	public class ApiKeyMiddleware
	{
		// TODO: Move this to another project (probably a datacontracts project) so it can be shared between the API and other consuming projects
		public const string API_KEY_HEADER_NAME = "X-API-Key";

		private readonly RequestDelegate _next;

		public ApiKeyMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			if (!context.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var extractedApiKey))
			{
				context.Response.StatusCode = 401;
				await context.Response.WriteAsync("Api Key was not provided.");
				return;
			}

			var configProvider = context.RequestServices.GetRequiredService<ConfigProviderService>();

			var apiKey = configProvider.ApiConfig.ApiKey!;

			if (!apiKey.Equals(extractedApiKey))
			{
				context.Response.StatusCode = 401;
				await context.Response.WriteAsync("Unauthorized client.");
				return;
			}

			await _next(context);
		}
	}
}