using BirthdayBot.Database;
using BirthdayBot.Database.Models;

namespace BirthdayBot.Services
{
    public class BirthdayService
    {
        private readonly BirthdayRepository _birthdayRepo;
        private readonly LoggingService _logger;
        private readonly GuildRepository _guildRepo;

        public BirthdayService(BirthdayRepository birthdayRepo, GuildRepository guildRepository, LoggingService logger)
        {
            _birthdayRepo = birthdayRepo;
            _logger = logger;
            _guildRepo = guildRepository;
        }

        public async Task AddBirthday(ulong guildId, ulong userId, int day, int month, ulong channelId)
        {
            if (!IsValidDate(day, month))
            {
                _logger.Warn($"Invalid birthday: {day}.{month}");
                return;
            }

            var birthday = new Birthday
            {
                GuildId = guildId,
                UserId = userId,
                Day = day,
                Month = month,
                ChannelId = channelId
            };

            await _birthdayRepo.AddBirthday(birthday);

            _logger.Info($"Birthday saved for {userId} in {guildId}");
        }

        public async Task<Birthday?> GetBirthday(ulong guildId, ulong userId)
        {
            var result = await _birthdayRepo.GetBirthday(guildId, userId);

            if (result == null)
                _logger.Warn($"No birthday found for {userId} in {guildId}");

            return result;
        }

        public async Task UpdateBirthday(Birthday birthday)
        {
            await _birthdayRepo.UpdateBirthday(birthday);
        }

        public async Task<List<Birthday>> GetBirthdays(ulong guildId)
        {
            return await _birthdayRepo.GetBirthdays(guildId);
        }

        public async Task<List<Birthday>> GetTodaysBirthdays(ulong guildId)
        {
            var today = DateTime.Today;

            return await _birthdayRepo.GetTodaysBirthdays(
                guildId,
                today.Day,
                today.Month);
        }

        public async Task<List<(Birthday entry, int daysLeft)>> GetUpcomingBirthdays(
            ulong guildId,
            int daysAhead = 30)
        {
            var birthdays = await _birthdayRepo.GetBirthdays(guildId);

            var today = DateTime.Today;

            var result = new List<(Birthday entry, int daysLeft)>();

            foreach (var b in birthdays)
            {
                var nextBirthday = new DateTime(today.Year, b.Month, b.Day);

                if (nextBirthday < today)
                    nextBirthday = nextBirthday.AddYears(1);

                var daysLeft = (nextBirthday - today).Days;

                if (daysLeft <= daysAhead)
                    result.Add((b, daysLeft));
            }

            return result
                .OrderBy(x => x.daysLeft)
                .ToList();
        }

        public async Task UpdateLastNotified(ulong guildId, ulong userId)
        {
            await _birthdayRepo.UpdateLastNotified(guildId, userId);
        }

        public async Task<bool> RemoveBirthday(ulong guildId, ulong userId)
        {
            return await _birthdayRepo.RemoveBirthday(guildId, userId);
        }

        public async Task<Guild?> GetConfig(ulong guildId)
        {
            var config = await _guildRepo.GetGuild(guildId);

            if (config == null)
            {
                _logger.Warn($"Guild config not found for {guildId}");
            }

            return config;
        }

        public async Task UpdateConfig(Guild config)
        {
            await _guildRepo.SaveGuild(config);

            _logger.Info($"Guild config updated for {config.GuildId}");
        }

        private bool IsValidDate(int day, int month)
        {
            if (month < 1 || month > 12)
                return false;

            if (day < 1 || day > 31)
                return false;

            return DateTime.TryParse($"{day}.{month}.2000", out _);
        }
    }
}