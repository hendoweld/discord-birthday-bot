using BirthdayBot.Services;
using Discord;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace BirthdayBot.Core
{
    public class BotEventHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly BirthdayBackgroundService _backgroundService;
        private readonly InteractionService _commands;
        private readonly BirthdayService _birthdayService;
        private readonly LoggingService _logger;
        private readonly BirthdayService _birthdayService;
        private readonly LoggingService _logger;

        public BotEventHandler(
            DiscordSocketClient client,
            BirthdayBackgroundService backgroundService,
            InteractionService commands,
            BirthdayService birthdayService,
            LoggingService logger)
            InteractionService commands,
            BirthdayService birthdayService,
            LoggingService logger)
        {
            _client = client;
            _backgroundService = backgroundService;
            _commands = commands;
            _birthdayService = birthdayService;
            _logger = logger;
            _birthdayService = birthdayService;
            _logger = logger;
        }

        public void Initialize()
        {
            _client.Ready += OnReady;
            _client.JoinedGuild += OnJoinedGuild;
            _client.LeftGuild += OnLeftGuild;
        }

        // BOT READY
        private async Task OnReady()
        {
            _logger.Info("READY EVENT TRIGGERED");
            await CleanupOrphanGuilds();
            foreach (var guild in _client.Guilds)
            {
                await _commands.RegisterCommandsToGuildAsync(guild.Id);
                _logger.Info($"Commands registriert für {guild.Name}");

                await AutoSetupGuild(guild);
                _logger.Info($"Commands registriert für {guild.Name}");

                await AutoSetupGuild(guild);
            }

            _backgroundService.Start();
        }

        // NEW SERVER
        private async Task OnJoinedGuild(SocketGuild guild)
        private async Task OnJoinedGuild(SocketGuild guild)
        {
            _logger.Info("======================");
            _logger.Info($"Joined guild: {guild.Name}");

            await AutoSetupGuild(guild);
        }

        //LEFT SERVER
        private async Task OnLeftGuild(SocketGuild guild)
        {
            _logger.Info("======================");
            _logger.Warn($"Bot removed from guild: {guild.Name} ({guild.Id})");

            try
            {
                await _birthdayService.DeleteGuildData(guild.Id);

                _logger.Info($"Guild data cleaned for {guild.Id}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Cleanup failed for {guild.Id}: {ex}");
            }
            _logger.Info("======================");
        }

        //LEFT SERVER WHILE OFFLINE
        private async Task CleanupOrphanGuilds()
        {
            _logger.Info("Checking orphan guilds...");

            var dbGuilds = await _birthdayService.GetAllGuildConfigs();

            foreach (var dbGuild in dbGuilds)
            {
                var existsInDiscord = _client.Guilds.Any(g => g.Id == dbGuild.GuildId);

                if (!existsInDiscord)
                {
                    _logger.Warn($"Guild not found in Discord anymore: {dbGuild.GuildId}");

                    await _birthdayService.DeleteGuildData(dbGuild.GuildId);
                }
            }

            _logger.Info("Orphan cleanup finished");
        }

        // AUTO SETUP
        private async Task AutoSetupGuild(SocketGuild guild)
        {
            _logger.Info("======================");
            _logger.Info($"AutoSetup start: {guild.Name}");

            try
            {
                var config = await _birthdayService.GetOrCreateConfig(guild.Id);

                // ROLE 
                _logger.Info("Checking birthday role");

                IRole role = null;

                if (config.BirthdayRoleId != 0)
                    role = guild.GetRole(config.BirthdayRoleId);
                    if (role == null || role.Name != "🎂 Birthday")
                    {
                        _logger.Warn($"Invalid role in DB: {role?.Name ?? "null"}");

                        config.BirthdayRoleId = 0;
                        role = null;
                }

                role ??= guild.Roles.FirstOrDefault(r => r.Name == "🎂 Birthday");

                if (role == null)
                {
                    _logger.Info("Birthday role not found -> creating role");

                    role = await guild.CreateRoleAsync(
                        "🎂 Birthday",
                        GuildPermissions.None,
                        Color.Magenta,
                        false,
                        false
                    );

                    await role.ModifyAsync(p =>
                    {
                        p.Position = guild.CurrentUser.Hierarchy - 1;
                    });

                    _logger.Info("Birthday role created");
                }

                config.BirthdayRoleId = role.Id;

                // CHANNEL
                _logger.Info("Checking birthday channel");

                ITextChannel channel = null;

                if (config.BirthdayChannelId != 0)
                {
                    channel = guild.GetTextChannel(config.BirthdayChannelId);
                }

                if (channel == null)
                {
                    channel = guild.TextChannels.FirstOrDefault(c => c.Name == "birthdays");

                    if (channel != null)
                        _logger.Info("Fallback channel 'birthdays' used");
                }

                if (channel == null)
                {
                    _logger.Warn("No birthday channel configured -> skipping guild");
                    return;
                }

                config.BirthdayChannelId = channel.Id;

                // ROLE HIERARCHY
                _logger.Info("Checking role hierarchy");

                var botUser = guild.CurrentUser;

                if (role.Position >= botUser.Hierarchy)
                {
                    _logger.Warn($"Role '{role.Name}' above bot -> trying to fix position");

                }
                else
                {
                    _logger.Info("Role hierarchy OK");
                }

                // SAVE
                _logger.Info("Saving guild config");

                await _birthdayService.UpdateConfig(config);

                _logger.Info($"AutoSetup finished: {guild.Name}");
            }
            catch (Exception ex)
            {
                _logger.Error($"AutoSetup failed for {guild.Name}: {ex.Message}");
            }
            _logger.Info("======================");
        }
    }
}