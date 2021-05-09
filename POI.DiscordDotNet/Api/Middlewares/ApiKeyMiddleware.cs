using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using POI.Core.Services;
using POI.DiscordDotNet.Services;

namespace POI.DiscordDotNet.Api.Middlewares
{
	public class ApiKeyMiddleware
	{
		private readonly RequestDelegate _next;

		public ApiKeyMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			if (!context.Request.Headers.TryGetValue(ConstantsCore.API_KEY_HEADER_NAME, out var extractedApiKey))
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