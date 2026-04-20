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
            var services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .AddSingleton<BirthdayService>()
                .AddSingleton<DiscordPermissionService>()
                .AddSingleton<BirthdayBackgroundService>()
                .AddSingleton<LoggingService>()
                .BuildServiceProvider();

            var birthdayService = services.GetRequiredService<BirthdayService>();
            birthdayService.Load();

            var backgroundService = services.GetRequiredService<BirthdayBackgroundService>();
            var logger = services.GetRequiredService<LoggingService>();

            var commandHandler = new CommandHandler(client, commands, services, logger);

            var eventHandler = new BotEventHandler(
                client,
                backgroundService,
                commands,
                birthdayService,
                logger
            );

            eventHandler.Initialize();

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            logger.Info("Bot gestartet");

            await commandHandler.InitializeAsync();

            await Task.Delay(-1);
        }
    }
}