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
        private const string FilePath = "birthdays.json";
        private List<Birthday> birthdays = new();

        public BirthdayService()
        {
            Load();
        }

        public void AddBirthday(ulong userId, int day, int month)
        {
            birthdays.RemoveAll(x => x.UserId == userId);

            birthdays.Add(new Birthday
            {
                UserId = userId,
                Day = day,
                Month = month
            });

            Save();
        }

        public List<Birthday> GetTodaysBirthdays()
        {
            var today = DateTime.Today;

            return birthdays
                .Where(x => x.Day == today.Day && x.Month == today.Month)
                .ToList();
        }

        private void Save()
        {
            File.WriteAllText(FilePath, JsonSerializer.Serialize(birthdays));
        }

        private void Load()
        {
            if (!File.Exists(FilePath)) return;

            birthdays = JsonSerializer.Deserialize<List<Birthday>>(File.ReadAllText(FilePath)) ?? new();
        }
    }
}