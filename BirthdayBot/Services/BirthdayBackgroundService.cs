using BirthdayBot.Database.Models;
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
                try
                {
                    await ProcessGuild(guild, today);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Guild '{guild.Name}' Fehler: {ex}");
                }
            }

            _logger.Info("Birthday check end");
        }

        private async Task ProcessGuild(
            SocketGuild guild, 
            DateTime today)
        {
            var config = await _birthdayService.GetOrCreateConfig(guild.Id);

            if (config == null)
                return;

            var channel = guild.GetTextChannel(config.BirthdayChannelId);

            if (channel == null)
                return;

            var role = guild.GetRole(config.BirthdayRoleId);

            if (role == null)
                return;

            if (!_permissionService.CanManageRole(guild, role))
                return;

            var birthdays = await _birthdayService.GetBirthdays(guild.Id);

            await CleanupBirthdayRoles(guild, role, birthdays, today);

            foreach (var birthday in birthdays.Where(x => x.Day == today.Day &&
                                                          x.Month == today.Month))
            {
                if (birthday.LastNotified?.Date == today)
                    continue;

                var user = guild.GetUser(birthday.UserId);

                if (user == null)
                    continue;

                await channel.SendMessageAsync($"🎉 <@{birthday.UserId}> hat heute Geburtstag!");

                await GiveBirthdayRole(guild, user, role);

                await _birthdayService.UpdateLastNotified(guild.Id, birthday.UserId);
            }
        }

        private async Task CleanupBirthdayRoles(
            SocketGuild guild,
            SocketRole role,
            IEnumerable<Birthday> birthdays,
            DateTime today)
        {
            var birthdayUsers = birthdays
                .Where(x => x.Day == today.Day && x.Month == today.Month)
                .Select(x => x.UserId)
                .ToHashSet();

            foreach (var user in guild.Users.Where(x => x.Roles.Contains(role)))
            {
                if (birthdayUsers.Contains(user.Id))
                    continue;

                try
                {
                    await user.RemoveRoleAsync(role);
                    _logger.Info($"Geburtstagsrolle entfernt von {user.Username}");
                }
                catch (Discord.Net.HttpException ex)
                {
                    _logger.Error($"RemoveRole: {ex.DiscordCode} - {ex.Reason}");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.ToString());
                }
            }
        }

        private async Task GiveBirthdayRole(
            SocketGuild guild,
            SocketGuildUser user,
            SocketRole role)
        {
            try
            {
                await user.AddRoleAsync(role);

                _logger.Info($"Geburtstagsrolle an {user.Username} vergeben.");
            }
            catch (Discord.Net.HttpException ex)
            {
                _logger.Error($"Discord Error {ex.DiscordCode}: {ex.Reason}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }
        }
    }
}