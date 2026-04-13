using BirthdayBot.Database.Models;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BirthdayBot.Services
{
    public class BirthdayService
    {
        private const string FilePath = "Data/birthdays.json";
        private List<Birthday> _birthdays = new();
        private List<GuildConfig> _configs = new();
        private readonly string _filePath = "config.json";
        private RootConfig _config;

        public BirthdayService()
        {
            Load();
        }

        public void AddBirthday(ulong guildId, ulong userId, int day, int month, ulong channelId)
        {
            var existing = _birthdays
                .FirstOrDefault(x => x.GuildId == guildId && x.UserId == userId);

            if (existing != null)
            {
                existing.Day = day;
                existing.Month = month;
                existing.ChannelId = channelId;
            }
            else
            {
                _birthdays.Add(new Birthday
                {
                    GuildId = guildId,
                    UserId = userId,
                    Day = day,
                    Month = month,
                    ChannelId = channelId
                });
            }

            Save();
        }

        public List<Birthday> GetBirthdays(ulong guildId)
        {
            return _birthdays
                .Where(x => x.GuildId == guildId)
                .OrderBy(x => x.Month)
                .ThenBy(x => x.Day)
                .ToList();
        }

        public bool RemoveBirthday(ulong guildId, ulong userId)
        {
            var entry = _birthdays
                .FirstOrDefault(x => x.GuildId == guildId && x.UserId == userId);

            if (entry == null)
                return false;

            _birthdays.Remove(entry);
            Save();

            return true;
        }
        public Birthday GetBirthday(ulong guildId, ulong userId)
        {
            return _birthdays
                .FirstOrDefault(x =>
                    x.GuildId == guildId &&
                    x.UserId == userId
                );
        }

        public List<Birthday> GetTodaysBirthdays(ulong guildId)
        {
            var today = DateTime.Now;

            return _birthdays
                .Where(x =>
                    x.GuildId == guildId &&
                    x.Day == today.Day &&
                    x.Month == today.Month)
                .ToList();
        }

        public List<(Birthday entry, int daysLeft)> GetUpcomingBirthdays(ulong guildId, int daysAhead = 30)
        {
            var today = DateTime.Today;

            var result = new List<(Birthday entry, int daysLeft)>();

            var birthdays = _birthdays.Where(x => x.GuildId == guildId);

            foreach (var b in birthdays)
            {
                var nextBirthday = new DateTime(today.Year, b.Month, b.Day);

                if (nextBirthday < today)
                    nextBirthday = nextBirthday.AddYears(1);

                var daysLeft = (nextBirthday - today).Days;

                if (daysLeft <= daysAhead)
                {
                    result.Add((b, daysLeft));
                }
            }

            return result
                .OrderBy(x => x.daysLeft)
                .ToList();
        }
        public GuildConfig GetConfig(ulong guildId)
        {
            return _config.GuildConfigs.FirstOrDefault(x => x.GuildId == guildId);
        }
        public GuildConfig GetOrCreateConfig(ulong guildId)
        {
            var config = GetConfig(guildId);

            if (config == null)
            {
                config = new GuildConfig
                {
                    GuildId = guildId
                };

                _config.GuildConfigs.Add(config);
            }

            return config;
        }

        public void UpdateConfig(GuildConfig config)
        {
            var existing = GetOrCreateConfig(config.GuildId);

            if (existing == null)
            {
                _config.GuildConfigs.Add(config);
            }
            else
            {
                existing.BirthdayChannelId = config.BirthdayChannelId;
                existing.BirthdayRoleId = config.BirthdayRoleId;
            }

            Save();
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_filePath, json);
            Directory.CreateDirectory("Data");

            File.WriteAllText(
                "Data/birthdays.json",
                JsonSerializer.Serialize(_birthdays, new JsonSerializerOptions
                {
                    WriteIndented = true
                })
            );
        }
        public void Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _config = new RootConfig();
                    return;
                }

                var json = File.ReadAllText(_filePath);

                _config = JsonSerializer.Deserialize<RootConfig>(json)
                          ?? new RootConfig();
                _birthdays = JsonSerializer.Deserialize<List<Birthday>>(json)
                         ?? new List<Birthday>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Config Load Error: {ex.Message}");
                _config = new RootConfig();
            }
        }

    }
}