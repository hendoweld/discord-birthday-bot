using BirthdayBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace BirthdayBot.Commands
{
    [Group("birthday", "Birthday commands")]
    public class BirthdayCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly BirthdayService _birthdayService;

        public BirthdayCommand(BirthdayService birthdayService)
        {
            _birthdayService = birthdayService;
        }

        // /birthday set
        [SlashCommand("set", "Speichert deinen Geburtstag")]
        public async Task Set(int day, int month)
        {
            if (!DateTime.TryParse($"{day}.{month}.2000", out _))
            {
                await RespondAsync("❌ Ungültiges Datum. Bitte prüfe Tag und Monat.", ephemeral: true);
                return;
            }

            await _birthdayService.AddBirthday(
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

        // /birthday me
        [SlashCommand("me", "Zeigt deinen Geburtstag")]
        public async Task Me()
        {
            var birthday = await _birthdayService.GetBirthday(
                Context.Guild.Id,
                Context.User.Id
            );

            if (birthday == null)
            {
                await RespondAsync("❌ Du hast noch keinen Geburtstag gesetzt.");
                return;
            }

            await RespondAsync($"📅 Dein Geburtstag: {birthday.Day:D2}.{birthday.Month:D2}");
        }

        // /birthday list
        [SlashCommand("list", "Zeigt alle gespeicherten Geburtstage")]
        public async Task List()
        {
            var birthdays = (await _birthdayService.GetBirthdays(Context.Guild.Id))
                .OrderBy(x => x.Month)
                .ThenBy(x => x.Day)
                .ToList();

            if (birthdays.Count == 0)
            {
                await RespondAsync("📭 Es sind noch keine Geburtstage gespeichert.", ephemeral: true);
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("🎂 Geburtstage auf diesem Server")
                .WithColor(Color.Gold);

            foreach (var b in birthdays)
            {
                var user = Context.Guild.GetUser(b.UserId);
                if (user == null) continue;

                embed.AddField(user.Username, $"📅 {b.Day:D2}.{b.Month:D2}", true);
            }

            await RespondAsync(embed: embed.Build());
        }

        // /birthday upcoming
        [SlashCommand("upcoming", "Zeigt kommende Geburtstage")]
        public async Task Upcoming()
        {
            var upcoming = await _birthdayService.GetUpcomingBirthdays(Context.Guild.Id);

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
                if (user == null) continue;

                var date = $"{item.entry.Day:D2}.{item.entry.Month:D2}";

                var text = item.daysLeft == 0
                    ? $"🎉 Heute! ({date})"
                    : $"in {item.daysLeft} Tagen ({date})";

                embed.AddField(user.Username, text, false);
            }

            await RespondAsync(embed: embed.Build());
        }

        // /birthday remove
        [SlashCommand("remove", "Löscht deinen gespeicherten Geburtstag")]
        public async Task Remove()
        {
            var removed = await _birthdayService.RemoveBirthday(
                Context.Guild.Id,
                Context.User.Id
            );

            if (!removed)
            {
                await RespondAsync("❌ Du hast noch keinen Geburtstag gespeichert.", ephemeral: true);
                return;
            }

            await RespondAsync("🗑️ Dein Geburtstag wurde gelöscht.", ephemeral: true);
        }

        // /birthday remove-user
        [SlashCommand("remove-user", "Löscht den Geburtstag eines Users")]
        [DefaultMemberPermissions(GuildPermission.Administrator)]
        public async Task RemoveUser(SocketGuildUser user)
        {
            var removed = await _birthdayService.RemoveBirthday(
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