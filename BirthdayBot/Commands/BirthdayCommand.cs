using BirthdayBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot.Commands
{
    public class BirthdayCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly BirthdayService _birthdayService;

        public BirthdayCommand(BirthdayService birthdayService)
        {
            _birthdayService = birthdayService;
        }

        [SlashCommand("birthday-set", "Speichert deinen Geburtstag")]
        public async Task Set(int day, int month)
        {
            if (!DateTime.TryParse($"{day}.{month}.2000", out _))
            {
                await RespondAsync(
                    "❌ Ungültiges Datum. Bitte prüfe Tag und Monat.",
                    ephemeral: true
                );
                return;
            }

            if (month < 1 || month > 12)
            {
                await RespondAsync("❌ Monat muss zwischen 1 und 12 liegen.", ephemeral: true);
                return;
            }

            if (day < 1 || day > 31)
            {
                await RespondAsync("❌ Tag muss zwischen 1 und 31 liegen.", ephemeral: true);
                return;
            }

            _birthdayService.AddBirthday(
                Context.Guild.Id,
                Context.User.Id,
                day,
                month,
                Context.Channel.Id
            );

            await RespondAsync(
                $"🎉 Geburtstag gespeichert!\n📅 {day:D2}.{month:D2}",
                ephemeral: true
            );
        }

        [SlashCommand("birthday-me", "Zeigt deinen Geburtstag")]
        public async Task Me()
        {
            var birthdays = _birthdayService.GetBirthdays(Context.Guild.Id);

            var user = birthdays.FirstOrDefault(x => x.UserId == Context.User.Id);

            if (user == null)
            {
                await RespondAsync("❌ Du hast noch keinen Geburtstag gesetzt.");
                return;
            }

            await RespondAsync($"📅 Dein Geburtstag: {user.Day}.{user.Month}");
        }

        [SlashCommand("birthday-list", "Zeigt alle gespeicherten Geburtstage")]
        public async Task List()
        {
            var birthdays = _birthdayService.GetBirthdays(Context.Guild.Id);

            if (birthdays.Count == 0)
            {
                await RespondAsync(
                    "📭 Es sind noch keine Geburtstage gespeichert.",
                    ephemeral: true
                );
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("🎂 Geburtstage auf diesem Server")
                .WithColor(Color.Gold);

            foreach (var b in birthdays)
            {
                var user = Context.Guild.GetUser(b.UserId);

                if (user == null)
                    continue;

                embed.AddField(
                    user.Username,
                    $"📅 {b.Day:D2}.{b.Month:D2}",
                    true
                );
            }

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("birthday-upcoming", "Zeigt kommende Geburtstage")]
        public async Task Upcoming()
        {
            var upcoming = _birthdayService.GetUpcomingBirthdays(Context.Guild.Id);

            if (upcoming.Count == 0)
            {
                await RespondAsync("📭 Keine Geburtstage in den nächsten 30 Tagen.");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("🎂 Kommende Geburtstage")
                .WithColor(Color.Purple);

            foreach (var item in upcoming)
            {
                var user = Context.Guild.GetUser(item.entry.UserId);

                if (user == null)
                    continue;

                var date = $"{item.entry.Day:D2}.{item.entry.Month:D2}";

                var text = item.daysLeft == 0
                    ? $"🎉 Heute! ({date})"
                    : $"in {item.daysLeft} Tagen ({date})";

                embed.AddField(user.Username, text, false);
            }

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("birthday-remove", "Löscht deinen gespeicherten Geburtstag")]
        public async Task Remove()
        {
            var removed = _birthdayService.RemoveBirthday(
                Context.Guild.Id,
                Context.User.Id
            );

            if (!removed)
            {
                await RespondAsync(
                    "❌ Du hast noch keinen Geburtstag gespeichert.",
                    ephemeral: true
                );
                return;
            }

            await RespondAsync(
                "🗑️ Dein Geburtstag wurde gelöscht.",
                ephemeral: true
            );
        }

        [SlashCommand("birthday-remove-user", "Löscht den Geburtstag eines Users")]
        [DefaultMemberPermissions(GuildPermission.Administrator)]
        public async Task RemoveUser(SocketGuildUser user)
        {
            var removed = _birthdayService.RemoveBirthday(
                Context.Guild.Id,
                user.Id
            );

            if (!removed)
            {
                await RespondAsync("❌ Dieser User hat keinen Geburtstag gespeichert.");
                return;
            }

            await RespondAsync($"🗑️ Geburtstag von {user.Username} gelöscht.");
        }
    }
}
