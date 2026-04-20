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

                    await Task.Delay(TimeSpan.FromMinutes(1), _cts.Token);
                }
            });
        }

        public void Stop()
        {
            _cts?.Cancel();
        }

        private async Task CheckBirthdays()
        {
            var today = DateTime.Today;
            _logger.Info("Birthday check start");

            foreach (var guild in _client.Guilds)
            {
                var config = await _birthdayService.GetConfig(guild.Id);

                if (config == null || config.BirthdayChannelId == 0)
                    continue;

                var channel = guild.GetTextChannel(config.BirthdayChannelId);
                if (channel == null)
                    continue;

                var role = guild.GetRole(config.BirthdayRoleId);
                if (role == null)
                    continue;

                if (!_permissionService.CanManageRole(guild, role))
                    continue;

                // ROLE CLEANUP
                var birthdays = await _birthdayService.GetBirthdays(guild.Id);

                var birthdayUserIds = birthdays
                    .Where(b => b.Day == today.Day && b.Month == today.Month)
                    .Select(b => b.UserId)
                    .ToHashSet();

                var roleId = role.Id;

                var membersWithRole = guild.Users
                    .Where(u => u.Roles.Any(r => r.Id == roleId));

                foreach (var user in membersWithRole)
                {
                    if (!birthdayUserIds.Contains(user.Id))
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

                foreach (var b in birthdays)
                {
                    if (b.LastNotified?.Date == today)
                        continue;

                    var user = guild.GetUser(b.UserId);
                    if (user == null)
                        continue;

                    await channel.SendMessageAsync($"🎉 <@{b.UserId}> hat heute Geburtstag!");

                    try
                    {
                        await user.AddRoleAsync(role);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"AddRole error: {ex.Message}");
                    }

                    await _birthdayService.UpdateLastNotified(guild.Id, b.UserId);
                }
            }

            _logger.Info("Birthday check end");
        }
    }
}