using BirthdayBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot
{
    public class Bot
    {
        private DiscordSocketClient _client;
        private InteractionService _commands;
        private IServiceProvider _services;

        private BirthdayService _birthdayService;
        private BirthdayReminderService _birthdayReminder;

        private LoggingService _loggingService;
        private CommandHandler _commandHandler;

        private bool _commandsRegistered = false;
        private ulong _roleId;
        public async Task RunAsync(string token)
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents =
                    GatewayIntents.Guilds |
                    GatewayIntents.GuildMessages |
                    GatewayIntents.MessageContent
            });

            _commands = new InteractionService(_client);

            _birthdayService = new BirthdayService();
            _birthdayService.Load();

            _services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .AddSingleton(_birthdayService)
            .BuildServiceProvider();

            _loggingService = new LoggingService(_client);

            _commandHandler = new CommandHandler(
                _client,
                _commands,
                _services
            );

            await _commandHandler.InitializeAsync();

            _client.Ready += OnReady;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task Ready()
        {
            foreach (var guild in _client.Guilds)
            {
                try
                {
                    var role = guild.Roles.FirstOrDefault(r => r.Name == "🎂 Birthday");

                    if (role == null)
                    {
                        var createdRole = await guild.CreateRoleAsync(
                            "🎂 Birthday",
                            GuildPermissions.None,
                            Color.Magenta,
                            false,
                            false
                        );

                        _roleId = createdRole.Id;
                        role = guild.GetRole(_roleId);
                    }
                    else
                    {
                        _roleId = role.Id;
                    }

                    var config = _birthdayService.GetOrCreateConfig(guild.Id);

                    _birthdayService.UpdateConfig(config);

                    config.BirthdayRoleId = role.Id;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Role error in {guild.Name}: {ex.Message}");
                }
            }

            Console.WriteLine("Bot ready auf allen Servers");
        }
        private async Task OnReady()
        {
            await _commands.RegisterCommandsGloballyAsync();
            await Ready();

            if (_birthdayReminder == null)
            {
                _birthdayReminder = new BirthdayReminderService(
                    _client,
                    _birthdayService,
                    _roleId
                );

                _birthdayReminder.Start();
            }

            Console.WriteLine("Reminder gestartet!");
        }
    }
}
