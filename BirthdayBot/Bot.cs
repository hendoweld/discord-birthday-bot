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
        public async Task RunAsync(string token)
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All
            });

            _commands = new InteractionService(_client);

            _birthdayService = new BirthdayService();

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

            _client.Ready += Ready;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task Ready()
        {
            if (!_commandsRegistered)
            {
                await _commands.RegisterCommandsGloballyAsync();
                _commandsRegistered = true;
            }

            _birthdayReminder = new BirthdayReminderService(
                _client,
                _birthdayService,
                123456789012345678
            );

            Console.WriteLine($"Bot online als {_client.CurrentUser}");
        }
    }
}
