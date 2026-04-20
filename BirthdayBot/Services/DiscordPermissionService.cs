using Discord;
using Discord.WebSocket;

namespace BirthdayBot.Services
{
    public class DiscordPermissionService
    {
        private readonly LoggingService _logger;

        public DiscordPermissionService(LoggingService logger)
        {
            _logger = logger;
        }

        public bool CanManageRole(SocketGuild guild, IRole role)
        {
            var botUser = guild.CurrentUser;

            // Bot braucht Manage Roles Permission
            if (!botUser.GuildPermissions.ManageRoles)
            {
                _logger.Warn($"Missing 'Manage Roles' in {guild.Name}");
                return false;
            }

            // Hierarchy Check
            if (role.Position >= botUser.Hierarchy)
            {
                _logger.Warn($"Role '{role.Name}' above bot hierarchy in {guild.Name}");
                return false;
            }

            return true;
        }
    }
}