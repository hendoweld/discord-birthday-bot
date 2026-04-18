using Discord;
using Discord.WebSocket;

namespace BirthdayBot.Services
{
    public class DiscordPermissionService
    {
        public bool CanManageRole(SocketGuild guild, IRole role)
        {
            var botUser = guild.CurrentUser;

            // Bot braucht Manage Roles Permission
            if (!botUser.GuildPermissions.ManageRoles)
            {
                Console.WriteLine($"Missing 'Manage Roles' in {guild.Name}");
                return false;
            }

            // Hierarchy Check
            if (role.Position >= botUser.Hierarchy)
            {
                Console.WriteLine($"Role '{role.Name}' above bot hierarchy in {guild.Name}");
                return false;
            }

            return true;
        }
    }
}