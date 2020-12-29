using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using PoiDiscordDotNet.Models.Configuration;
using Serilog;

namespace PoiDiscordDotNet.Services
{
	public class ConfigProviderService
	{
		internal const string CONFIG_FILE_NAME = "configuration.json";

		private readonly ILogger _logger;
		private readonly string _configPath;

		private Configuration? _configuration;

		internal DiscordConfig Discord => _configuration!.DiscordConfig!;
		internal MongoDbConfig MongoDb => _configuration!.MongoDbConfig!;

		public ConfigProviderService(ILogger logger, string configPath)
		{
			_logger = logger;
			_configPath = configPath;
		}

		internal async Task<bool> LoadConfig()
		{
			_logger.Information($"Configuration file path: {_configPath}");

			if (File.Exists(_configPath))
			{
				await using var utf8Stream = File.OpenRead(_configPath);
				try
				{
					_configuration = await JsonSerializer.DeserializeAsync<Configuration>(utf8Stream).ConfigureAwait(false);
					return ValidateConfig();
				}
				catch (NotSupportedException)
				{
					_logger.Fatal("The content type is not supported.");
				}
				catch (JsonException)
				{
					_logger.Fatal("Invalid configuration json file.");
				}
			}
			else
			{
				_logger.Fatal("Configuration json file was not found.");
			}

			return false;
		}

		/// <summary>
		/// Validates the loaded config
		/// </summary>
		/// <returns>A boolean indicating whether the config is valid or not</returns>
		private bool ValidateConfig()
		{
			if (_configuration == null)
			{
				_logger.Error("Configuration was null. Base validation failed.");
				return false;
			}

			if (_configuration.DiscordConfig == null || !ValidateDiscordConfig())
			{
				_logger.Error("Discord configuration validation failed.");
				return false;
			}

			if (_configuration.MongoDbConfig == null || !ValidateMongoDbConfig())
			{
				_logger.Error("MongoDb configuration validation failed.");
				return false;
			}

			return true;
		}

		private bool ValidateDiscordConfig()
		{
			if (string.IsNullOrWhiteSpace(Discord.Token))
			{
				_logger.Error("Token is null, empty or whitespace. Validation failed.");
				return false;
			}

			if (Discord.Prefix == null)
			{
				_logger.Error("Prefix is null. Validation failed.");
				return false;
			}

			return true;
		}

		private bool ValidateMongoDbConfig()
		{
			if (string.IsNullOrWhiteSpace(MongoDb.MongoDbConnectionString))
			{
				_logger.Error("MongoDbConnectionString is null, empty or whitespace. Validation failed.");
				return false;
			}

			return true;
		}
	}
}