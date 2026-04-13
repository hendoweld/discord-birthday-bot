using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BirthdayBot.Config
{
    public static class ConfigLoader
    {
        public static BotConfig Load()
        {
            // 1. Erst ENV prüfen (Production)
            var envToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN");

            if (!string.IsNullOrWhiteSpace(envToken))
            {
                Console.WriteLine("Config: Token aus ENV geladen");
                return new BotConfig
                {
                    Token = envToken
                };
            }

            // 2. Fallback: JSON Datei (Development)
            var path = Path.Combine(AppContext.BaseDirectory, "Config/config.json");

            if (!File.Exists(path))
                throw new Exception("config.json nicht gefunden!");

            var json = File.ReadAllText(path);

            var config = JsonSerializer.Deserialize<BotConfig>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (string.IsNullOrWhiteSpace(config?.Token))
                throw new Exception("Token fehlt in Config!");

            Console.WriteLine("Config: Token aus JSON geladen");
            return config;
        }
    }
}
