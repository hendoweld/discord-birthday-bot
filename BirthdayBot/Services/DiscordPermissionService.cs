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

            if (!botUser.GuildPermissions.ManageRoles)
            {
                _logger.Warn($"Missing 'Manage Roles' in {guild.Name}");
                return false;
            }

            if (role.Position >= botUser.Hierarchy)
            {
                _logger.Warn($"Role '{role.Name}' above bot hierarchy in {guild.Name}");

                LogRoleDiagnostics(guild, role);

                return false;
            }

            return true;
        }

        public void LogRoleDiagnostics(SocketGuild guild, IRole role)
        {
            var botUser = guild.CurrentUser;

            _logger.Warn("=== ROLE DIAGNOSTICS ===");
            _logger.Warn($"Guild: {guild.Name}");
            _logger.Warn($"Bot Role Position: {botUser.Hierarchy}");
            _logger.Warn($"Target Role: {role.Name}");
            _logger.Warn($"Target Role Position: {role.Position}");

            if (role.Position >= botUser.Hierarchy)
            {
                _logger.Warn("❌ ISSUE: Role is ABOVE bot hierarchy");
                _logger.Warn("➡ Fix: Move bot role ABOVE the target role in Discord settings");
            }
            else
            {
                _logger.Info("✅ Role hierarchy is OK");
            }
        }
    }
}