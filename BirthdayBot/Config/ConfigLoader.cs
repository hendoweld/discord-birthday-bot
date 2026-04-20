using BirthdayBot.Database.Models;
using BirthdayBot.Services;
using System.Text.Json;

namespace BirthdayBot.Config
{
    public class ConfigLoader
    {
        private readonly LoggingService _logger;

        public ConfigLoader(LoggingService logger)
        {
            _logger = logger;
        }

        public BotConfig Load()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Config/config.json");
            _logger.Info($"Config Path: {path}");

            if (!File.Exists(path))
                throw new Exception("config.json nicht gefunden!");

            var json = File.ReadAllText(path);

            var config = JsonSerializer.Deserialize<BotConfig>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new Exception("Config konnte nicht gelesen werden!");

            var envToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN");

            if (!string.IsNullOrWhiteSpace(envToken))
            {
                _logger.Info("Config: Token aus ENV geladen");
            }

            if (string.IsNullOrWhiteSpace(config?.Token))
                throw new Exception("Token fehlt in Config!");

            if (string.IsNullOrWhiteSpace(config.Database))
                throw new Exception("Database fehlt in Config!");

            _logger.Info("Config geladen");
            return config;
        }
    }
}