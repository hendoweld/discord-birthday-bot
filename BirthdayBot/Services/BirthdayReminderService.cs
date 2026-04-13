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
        private readonly ulong _channelId;

        public BirthdayReminderService(
            DiscordSocketClient client,
            BirthdayService birthdayService,
            ulong channelId)
        {
            _client = client;
            _birthdayService = birthdayService;
            _channelId = channelId;

            StartScheduler();
        }

        private void StartScheduler()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var now = DateTime.Now;
                    var nextRun = DateTime.Today.AddDays(1);

                    var delay = nextRun - now;

                    await Task.Delay(delay);

                    await CheckBirthdays();
                }
            });
        }

        private async Task CheckBirthdays()
        {
            var birthdays = _birthdayService.GetTodaysBirthdays();

            var channel = _client.GetChannel(_channelId) as ISocketMessageChannel;

            foreach (var entry in birthdays)
            {
                var user = _client.GetUser(entry.UserId);

                if (user != null)
                {
                    await channel.SendMessageAsync(
                        $"🎉 Heute hat {user.Mention} Geburtstag!");
                }
            }
        }
    }
}
