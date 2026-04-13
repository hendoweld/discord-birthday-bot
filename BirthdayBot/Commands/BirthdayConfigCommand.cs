using BirthdayBot.Services;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot.Commands
{
    public class BirthdayConfigCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly BirthdayService _birthdayService;

        public BirthdayConfigCommand(BirthdayService birthdayService)
        {
            _birthdayService = birthdayService;
        }
        [SlashCommand("birthday-config-channel", "Setzt den Birthday Channel")]
        public async Task SetChannel(ITextChannel channel)
        {
            var config = _birthdayService.GetOrCreateConfig(Context.Guild.Id);

            config.BirthdayChannelId = channel.Id;

            _birthdayService.UpdateConfig(config);

            if (config == null)
            {
                await RespondAsync("❌ Bitte zuerst Role setzen!", ephemeral: true);
                Console.WriteLine($"GuildId: {Context.Guild.Id}");
                return;
            }

            config.BirthdayChannelId = channel.Id;
            _birthdayService.Save();

            await RespondAsync($"✅ Birthday Channel gesetzt: {channel.Mention}", ephemeral: true);
        }
        [SlashCommand("birthday-config-role", "Setzt die Birthday Role")]
        public async Task SetRole(IRole role)
        {
            var config = _birthdayService.GetOrCreateConfig(Context.Guild.Id);

            config.BirthdayRoleId = role.Id;

            _birthdayService.UpdateConfig(config);

            if (config == null)
            {
                await RespondAsync("❌ Config existiert nicht!", ephemeral: true);
                Console.WriteLine($"GuildId: {Context.Guild.Id}");
                return;
            }

            config.BirthdayRoleId = role.Id;
            _birthdayService.Save();

            await RespondAsync($"🎂 Role gesetzt: {role.Name}", ephemeral: true);
        }
    }
}
