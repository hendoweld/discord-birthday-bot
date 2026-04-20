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
            var envToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN");

            if (!string.IsNullOrWhiteSpace(envToken))
            {
                _logger.Info("Config: Token aus ENV geladen");
                return new BotConfig { Token = envToken };
            }

            var path = Path.Combine(AppContext.BaseDirectory, "Config/config.token.json");
            _logger.Info($"Config Path: {path}");

            if (!File.Exists(path))
                throw new Exception("config.token.json nicht gefunden!");

            var json = File.ReadAllText(path);

            var config = JsonSerializer.Deserialize<BotConfig>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new Exception("Config konnte nicht gelesen werden!");

            if (string.IsNullOrWhiteSpace(config?.Token))
                throw new Exception("Token fehlt in Config!");

            _logger.Info("Config: Token aus JSON geladen");
            return config;
        }
    }
}