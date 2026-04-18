using BirthdayBot.Core;
using BirthdayBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace BirthdayBot
{
    public class Bot
    {
        public async Task RunAsync(string token)
        {
            var client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents =
                    GatewayIntents.Guilds |
                    GatewayIntents.GuildMembers |
                    GatewayIntents.GuildMessages |
                    GatewayIntents.MessageContent
            });

            var commands = new InteractionService(client);

            // SERVICES
            var birthdayService = new BirthdayService();
            birthdayService.Load();

            var permissionService = new DiscordPermissionService();

            var backgroundService = new BirthdayBackgroundService(
                client,
                birthdayService,
                permissionService
            );

            // COMMAND HANDLER
            var services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .AddSingleton(birthdayService)
                .BuildServiceProvider();

            var commandHandler = new CommandHandler(client, commands, services);

            // EVENT HANDLER
            var eventHandler = new BotEventHandler(
                client,
                backgroundService,
                commands
            );

            eventHandler.Initialize();

            // LOGIN FIRST
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            Console.WriteLine("Bot gestartet");

            // THEN INIT COMMANDS
            await commandHandler.InitializeAsync();

            await Task.Delay(-1);
        }
    }
}