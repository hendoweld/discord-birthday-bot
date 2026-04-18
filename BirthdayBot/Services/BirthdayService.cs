using BirthdayBot.Database.Models;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BirthdayBot.Services
{
    public class BirthdayService
    {
        private Root _config;

        public BirthdayService()
        {
            Load();
        }

        public void AddBirthday(ulong guildId, ulong userId, int day, int month, ulong channelId)
        {
            // VALIDATION
            if (!IsValidDate(day, month))
            {
                Console.WriteLine($"Invalid birthday: {day}.{month}");
                return;
            }

            // existing entry finden
            var existing = _config.Birthdays
                .FirstOrDefault(x => x.GuildId == guildId && x.UserId == userId);

            if (existing != null)
            {
                // update
                existing.Day = day;
                existing.Month = month;
                existing.ChannelId = channelId;
            }
            else
            {
                // insert
                _config.Birthdays.Add(new Birthday
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

        public Birthday? GetBirthday(ulong guildId, ulong userId)
        {
            var result = _config.Birthdays
                .FirstOrDefault(x =>
                    x.GuildId == guildId &&
                    x.UserId == userId);

            if (result == null)
                Console.WriteLine($"No birthday found for {userId} in {guildId}");

            return result;
        }

        public List<Birthday> GetBirthdays(ulong guildId)
        {
            return _config.Birthdays
                .Where(x => x.GuildId == guildId)
                .OrderBy(x => x.Month)
                .ThenBy(x => x.Day)
                .ToList();
        }

        public List<Birthday> GetTodaysBirthdays(ulong guildId)
        {
            var today = DateTime.Today;

            return _config.Birthdays
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

            var birthdays = _config.Birthdays
                .Where(x => x.GuildId == guildId);

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

        public bool RemoveBirthday(ulong guildId, ulong userId)
        {
            var entry = _config.Birthdays
                .FirstOrDefault(x => x.GuildId == guildId && x.UserId == userId);

            if (entry == null)
                return false;

            _config.Birthdays.Remove(entry);
            Save();

            return true;
        }

        public Guild GetConfig(ulong guildId)
        {
            return _config.Guilds.FirstOrDefault(x => x.GuildId == guildId);
        }
        public Guild GetOrCreateConfig(ulong guildId)
        {
            var config = GetConfig(guildId);

            if (config == null)
            {
                config = new Guild
                {
                    GuildId = guildId
                };

                _config.Guilds.Add(config);
            }

            return config;
        }

        public void UpdateConfig(Guild config)
        {
            var existing = GetOrCreateConfig(config.GuildId);

            if (existing == null)
            {
                _config.Guilds.Add(config);
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
            Directory.CreateDirectory("Data");

            var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText("Data/config.lists.json", json);
        }
        public void Load()
        {
            try
            {
                var path = "Data/config.lists.json";

                if (!File.Exists(path))
                {
                    _config = new Root();
                    return;
                }

                var json = File.ReadAllText(path);

                _config = JsonSerializer.Deserialize<Root>(json) ?? new Root();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load Error: {ex.Message}");
                _config = new Root();
            }
        }

        private bool IsValidDate(int day, int month)
        {
            if (month < 1 || month > 12)
                return false;

            if (day < 1 || day > 31)
                return false;

            // echte Kalenderprüfung
            return DateTime.TryParse($"{day}.{month}.2000", out _);
        }
    }
}