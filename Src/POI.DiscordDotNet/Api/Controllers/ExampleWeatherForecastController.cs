using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POI.DiscordDotNet.Api.Responses;

namespace POI.DiscordDotNet.Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ExampleWeatherForecastController : ControllerBase
	{
		private static readonly string[] Summaries = {"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"};

		private readonly ILogger<ExampleWeatherForecastController> _logger;

		public ExampleWeatherForecastController(ILogger<ExampleWeatherForecastController> logger)
		{
			_logger = logger;
		}

		[HttpGet]
		public IEnumerable<WeatherForecast> Get()
		{
			_logger.LogInformation("Data decided by rng. Let's hope it's in your favor");

			var rng = new Random();
			return Enumerable.Range(1, 5).Select(index => new WeatherForecast(
					DateTime.Now.AddDays(index),
					rng.Next(-20, 55),
					Summaries[rng.Next(Summaries.Length)])
				)
				.ToArray();
		}
	}
}