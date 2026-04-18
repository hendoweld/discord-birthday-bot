using BirthdayBot.Services;
using Discord;
using Discord.Interactions;

namespace BirthdayBot.Commands
{
    [Group("birthday-config", "Birthday configuration commands")]
    [DefaultMemberPermissions(GuildPermission.Administrator)] // nur Admins
    public class BirthdayConfigCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly BirthdayService _birthdayService;

        public BirthdayConfigCommand(BirthdayService birthdayService)
        {
            _birthdayService = birthdayService;
        }

        // /birthday-config channel
        [SlashCommand("channel", "Setzt den Birthday Channel")]
        public async Task SetChannel(ITextChannel channel)
        {
            var config = _birthdayService.GetOrCreateConfig(Context.Guild.Id);

            config.BirthdayChannelId = channel.Id;

            _birthdayService.UpdateConfig(config);
            _birthdayService.Save();

            await RespondAsync($"✅ Channel gesetzt: {channel.Mention}", ephemeral: true);
        }

        // /birthday-config role
        [SlashCommand("role", "Setzt die Birthday Role")]
        public async Task SetRole(IRole role)
        {
            var config = _birthdayService.GetOrCreateConfig(Context.Guild.Id);

            config.BirthdayRoleId = role.Id;

            _birthdayService.UpdateConfig(config);
            _birthdayService.Save();

            await RespondAsync($"🎂 Role gesetzt: {role.Name}", ephemeral: true);
        }

        // /birthday-config show
        [SlashCommand("show", "Zeigt aktuelle Birthday Config")]
        public async Task Show()
        {
            var config = _birthdayService.GetOrCreateConfig(Context.Guild.Id);

            if (config == null)
            {
                await RespondAsync("❌ Keine Config gefunden", ephemeral: true);
                return;
            }

            var channel = Context.Guild.GetTextChannel(config.BirthdayChannelId);
            var role = Context.Guild.GetRole(config.BirthdayRoleId);

            var embed = new EmbedBuilder()
                .WithTitle("⚙️ Birthday Config")
                .WithColor(Color.Blue)
                .AddField("Channel", channel != null ? channel.Mention : "❌ nicht gesetzt", true)
                .AddField("Role", role != null ? role.Name : "❌ nicht gesetzt", true)
                .Build();

            await RespondAsync(embed: embed, ephemeral: true);
        }

        // /birthday-config reset
        [SlashCommand("reset", "Setzt die Config zurück")]
        public async Task Reset()
        {
            var config = _birthdayService.GetOrCreateConfig(Context.Guild.Id);

            config.BirthdayChannelId = 0;
            config.BirthdayRoleId = 0;

            _birthdayService.UpdateConfig(config);
            _birthdayService.Save();

            await RespondAsync("🧹 Config wurde zurückgesetzt", ephemeral: true);
        }
    }
}