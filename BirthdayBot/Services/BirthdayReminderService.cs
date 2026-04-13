using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot.Services
{
    public class BirthdayReminderService
    {
        private readonly DiscordSocketClient _client;
        private readonly BirthdayService _birthdayService;
        private ulong _roleId;

        private Timer _timer;

        public BirthdayReminderService(
            DiscordSocketClient client,
            BirthdayService birthdayService,
            ulong roleId
            )
        {
            _client = client;
            _birthdayService = birthdayService;
            _roleId = roleId;
            Console.WriteLine("Reminder Service gestartet");
        }

        public void Start()
        {
            _timer = new Timer(async _ =>
            {
                Console.WriteLine("Reminder Tick läuft");
                await CheckBirthdays();
            },
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(1));
        }

        public async Task CheckBirthdays()
        {
            var today = DateTime.Today;

            foreach (var guild in _client.Guilds)
            {
                var config = _birthdayService.GetOrCreateConfig(guild.Id);
                if (config == null || config.BirthdayChannelId == 0)
                {
                    Console.WriteLine($"No config for {guild.Name}");
                    continue;
                }
                if (config.BirthdayChannelId == 0)
                {
                    Console.WriteLine($"No channel set for {guild.Name}");
                    continue;
                }

                var channel = guild.GetTextChannel(config.BirthdayChannelId);

                if (channel == null)
                {
                    Console.WriteLine("Channel not found");
                    continue;
                }

                var role = guild.GetRole(config.BirthdayRoleId);

                foreach (var user in guild.Users)
                {
                    if (role == null) continue;

                    var birthday = _birthdayService.GetBirthday(guild.Id, user.Id);

                    if (user.Roles.Any(r => r.Id == role.Id))
                    {
                        if (birthday == null ||
                            birthday.Day != today.Day ||
                            birthday.Month != today.Month)
                        {
                            try
                            {
                                await user.RemoveRoleAsync(role.Id);
                            }
                            catch (Discord.Net.HttpException ex)
                            {
                                Console.WriteLine($"Role error: {ex.Message}");
                            }
                        }
                    }
                }

                if (channel == null) continue;

                var birthdays = _birthdayService.GetTodaysBirthdays(guild.Id);

                foreach (var b in birthdays)
                {
                    if (b.LastNotified.HasValue &&
                        b.LastNotified.Value.Date == today)
                        continue;

                    await channel.SendMessageAsync(
                        $"🎉 <@{b.UserId}> hat heute Geburtstag!"
                    );

                    if (role != null)
                    {
                        var user = guild.GetUser(b.UserId);
                        if (user != null)
                        {
                            var botUser = guild.CurrentUser;

                            if (role.Position >= botUser.Hierarchy)
                            {
                                Console.WriteLine("❌ Role ist über Bot-Hierarchie!");
                                continue;
                            }
                            try
                            {
                                await user.AddRoleAsync(role);
                            }
                            catch (Discord.Net.HttpException ex)
                            {
                                Console.WriteLine($"Role error: {ex.Message}");
                            }
                        }
                    }

                    b.LastNotified = today;
                }
            }

            _birthdayService.Save();
        }
    }
}
