using BirthdayBot.Core;
using BirthdayBot.Services;
using BirthdayBot.Database;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using BirthdayBot.Database.Models;

namespace BirthdayBot
{
    public class Bot
    {
        public async Task RunAsync(BotConfig config, LoggingService logger)
        {

            // DISCORD CLIENT
            var client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents =
                    GatewayIntents.Guilds |
                    GatewayIntents.GuildMembers |
                    GatewayIntents.GuildMessages |
                    GatewayIntents.MessageContent
            });

            var commands = new InteractionService(client);

            // DATABASE
            var database = new DatabaseService(config.Database, logger);
            await database.Initialize();

            // DEPENDENCY INJECTION
            var services = new ServiceCollection()
                .AddSingleton(logger)
                .AddSingleton(database)
                .AddSingleton(client)
                .AddSingleton(commands)

                .AddSingleton<BirthdayRepository>()
                .AddSingleton<GuildRepository>()

                .AddSingleton<BirthdayService>()
                .AddSingleton<DiscordPermissionService>()
                .AddSingleton<BirthdayBackgroundService>()

                .BuildServiceProvider();

            // RESOLVE SERVICES
            var birthdayService = services.GetRequiredService<BirthdayService>();
            var backgroundService = services.GetRequiredService<BirthdayBackgroundService>();

            var commandHandler = new CommandHandler(client, commands, services, logger);

            var eventHandler = new BotEventHandler(
                client,
                backgroundService,
                commands,
                birthdayService,
                logger
            );

            eventHandler.Initialize();

            // START BOT
            await client.LoginAsync(TokenType.Bot, config.Token);
            await client.StartAsync();

            logger.Info("Bot gestartet");

            await commandHandler.InitializeAsync();

            await Task.Delay(-1);
        }
    }
}