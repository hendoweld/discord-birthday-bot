using Discord.WebSocket;

namespace BirthdayBot.Services
{
    public class BirthdayBackgroundService
    {
        private readonly DiscordSocketClient _client;
        private readonly BirthdayService _birthdayService;
        private readonly DiscordPermissionService _permissionService;
        private readonly LoggingService _logger;

        private CancellationTokenSource _cts;

        public BirthdayBackgroundService(
            DiscordSocketClient client,
            BirthdayService birthdayService,
            DiscordPermissionService permissionService,
            LoggingService logger)
        {
            _client = client;
            _birthdayService = birthdayService;
            _permissionService = permissionService;
            _logger = logger;
        }

        // START LOOP
        public void Start()
        {
            _cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                _logger.Info("Birthday Background Service gestartet");

                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await CheckBirthdays();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Background error: {ex.Message}");
                    }

                    // Intervall
                    await Task.Delay(TimeSpan.FromMinutes(1), _cts.Token);
                }
            });
        }

        // STOP LOOP
        public void Stop()
        {
            _cts?.Cancel();
        }

        // MAIN LOGIC
        private async Task CheckBirthdays()
        {
            var today = DateTime.Today;
            _logger.Info($"Birthday check start");

            foreach (var guild in _client.Guilds)
            {
                var config = _birthdayService.GetOrCreateConfig(guild.Id);

                if (config.BirthdayChannelId == 0)
                    continue;

                var channel = guild.GetTextChannel(config.BirthdayChannelId);
                if (channel == null)
                    continue;

                var role = guild.GetRole(config.BirthdayRoleId);
                if (role == null)
                    continue;

                // Permission Check
                if (!_permissionService.CanManageRole(guild, role))
                    continue;

                // ROLE CLEANUP
                foreach (var user in guild.Users.Where(u => u.Roles.Any(r => r.Id == role.Id)))
                {
                    var birthday = _birthdayService.GetBirthday(guild.Id, user.Id);

                    if (birthday == null ||
                        birthday.Day != today.Day ||
                        birthday.Month != today.Month)
                    {
                        try
                        {
                            await user.RemoveRoleAsync(role);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"RemoveRole error: {ex.Message}");
                        }
                    }
                }

                // TODAY BIRTHDAYS
                var birthdays = _birthdayService.GetTodaysBirthdays(guild.Id);

                foreach (var b in birthdays)
                {
                    if (b.LastNotified?.Date == today)
                        continue;

                    var user = guild.GetUser(b.UserId);
                    if (user == null)
                        continue;

                    await channel.SendMessageAsync(
                        $"🎉 <@{b.UserId}> hat heute Geburtstag!"
                    );

                    try
                    {
                        await user.AddRoleAsync(role);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"AddRole error: {ex.Message}");
                    }

                    b.LastNotified = today;
                }
            }
            _logger.Info($"Birthday check end");
            _birthdayService.Save();
        }
    }
}