using BirthdayBot.Services;
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

        public BotEventHandler(
            DiscordSocketClient client,
            BirthdayBackgroundService backgroundService,
            InteractionService commands,
            BirthdayService birthdayService,
            LoggingService logger)
        {
            _client = client;
            _backgroundService = backgroundService;
            _commands = commands;
            _birthdayService = birthdayService;
            _logger = logger;
        }

        public void Initialize()
        {
            _client.Ready += OnReady;
            _client.JoinedGuild += OnJoinedGuild;
        }

        // BOT READY
        private async Task OnReady()
        {
            _logger.Info("READY EVENT TRIGGERED");

            foreach (var guild in _client.Guilds)
            {
                await _commands.RegisterCommandsToGuildAsync(guild.Id);
                _logger.Info($"Commands registriert für {guild.Name}");

                await AutoSetupGuild(guild);
            }

            _backgroundService.Start();
        }

        // NEW SERVER
        private async Task OnJoinedGuild(SocketGuild guild)
        {
            _logger.Info($"Joined guild: {guild.Name}");

            await EnsureBirthdaySetup(guild);
        }

        // AUTO SETUP
        private async Task EnsureBirthdaySetup(SocketGuild guild)
        {
            _logger.Info($"Checking setup for {guild.Name}");

            IRole role = guild.Roles.FirstOrDefault(r => r.Name == "🎂 Birthday");

            // Rolle erstellen falls sie fehlt
            if (role == null)
            {
                role = await guild.CreateRoleAsync(
                    "🎂 Birthday",
                    GuildPermissions.None,
                    Color.Magenta,
                    false,
                    false
                );

                _logger.Info("Birthday role created");
            }

            // Hierarchy Check
            var botPosition = guild.CurrentUser.Hierarchy;

            if (role.Position >= botPosition)
            {
                try
                {
                    await role.ModifyAsync(x =>
                    {
                        x.Position = botPosition - 1;
                    });

                    _logger.Info("Birthday role repositioned");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Role position error: {ex.Message}");
                }
            }

            // Config speichern
            var config = _birthdayService.GetOrCreateConfig(guild.Id);

            config.BirthdayRoleId = role.Id;

            _birthdayService.UpdateConfig(config);
            _birthdayService.Save();

            _logger.Info("Birthday config updated");
        }

        private async Task AutoSetupGuild(SocketGuild guild)
        {
            var config = _birthdayService.GetOrCreateConfig(guild.Id);

            _logger.Info($"AutoSetup für {guild.Name}");

            // ROLE
            IRole role = guild.GetRole(config.BirthdayRoleId);

            if (role == null)
            {
                role = guild.Roles.FirstOrDefault(r => r.Name == "🎂 Birthday");

                if (role == null)
                {
                    role = await guild.CreateRoleAsync(
                        "🎂 Birthday",
                        GuildPermissions.None,
                        Color.Magenta,
                        false,
                        false
                    );

                    _logger.Info($"Role erstellt: {role.Name}");
                }

                config.BirthdayRoleId = role.Id;
            }

            // CHANNEL
            ITextChannel? channel = guild.GetTextChannel(config.BirthdayChannelId);

            if (channel == null)
            {
                channel = guild.TextChannels
                    .FirstOrDefault(c => c.Name == "birthdays");

                if (channel == null)
                {
                    channel = await guild.CreateTextChannelAsync("birthdays");
                    _logger.Info($"Channel erstellt: {channel.Name}");
                }

                config.BirthdayChannelId = channel.Id;
            }

            // ROLE HIERARCHY FIX
            var botUser = guild.CurrentUser;

            if (role.Position >= botUser.Hierarchy)
            {
                _logger.Info("Birthday Role über Bot reposition");

                try
                {
                    await role.ModifyAsync(p =>
                    {
                        p.Position = botUser.Hierarchy - 1;
                    });
                }
                catch (Exception ex)
                {
                    _logger.Error($"Role position error: {ex.Message}");
                }
            }

            _birthdayService.UpdateConfig(config);
            _birthdayService.Save();
        }
    }
}