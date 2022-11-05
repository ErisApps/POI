namespace POI.DiscordDotNet.Api.Responses
{
	public class WeatherForecast
	{
		public WeatherForecast(DateTime date, int temperatureC, string summary)
		{
			Date = date;
			TemperatureC = temperatureC;
			Summary = summary;
		}
		public DateTime Date { get; init; }

		public int TemperatureC { get; init; }

		public int TemperatureF => 32 + (int) (TemperatureC / 0.5556);

		public string Summary { get; init; }
	}
}