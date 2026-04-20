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

            await AutoSetupGuild(guild);
        }

        // AUTO SETUP
        private async Task AutoSetupGuild(SocketGuild guild)
        {
            _logger.Info($"AutoSetup start: {guild.Name}");

            try
            {
                var config = await _birthdayService.GetConfig(guild.Id);

                if (config == null)
                {
                    config = new Database.Models.Guild
                    {
                        GuildId = guild.Id
                    };

                    _logger.Info("No config found -> new config created");
                }

                // ROLE 
                _logger.Info("Checking birthday role");

                IRole role = null;

                if (config.BirthdayRoleId != 0)
                    role = guild.GetRole(config.BirthdayRoleId);

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
                    channel = guild.GetTextChannel(config.BirthdayChannelId);

                channel ??= guild.TextChannels.FirstOrDefault(c => c.Name == "birthdays");

                if (channel == null)
                {
                    _logger.Info("Birthday channel not found -> creating channel");

                    channel = await guild.CreateTextChannelAsync("birthdays");

                    _logger.Info("Birthday channel created");
                }

                config.BirthdayChannelId = channel.Id;

                // ROLE HIERARCHY
                _logger.Info("Checking role hierarchy");

                var botUser = guild.CurrentUser;

                if (role.Position >= botUser.Hierarchy)
                {
                    _logger.Warn($"Role '{role.Name}' above bot -> trying to fix position");

                    try
                    {
                        await role.ModifyAsync(p =>
                        {
                            p.Position = botUser.Hierarchy - 1;
                        });

                        _logger.Info("Birthday role repositioned successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Role reposition failed: {ex.Message}");
                    }
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
        }
    }
}