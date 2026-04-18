using BirthdayBot.Services;
using Discord.Interactions;
using Discord.WebSocket;

namespace BirthdayBot.Core
{
    public class BotEventHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly BirthdayBackgroundService _backgroundService;
        private readonly InteractionService _commands;

        public BotEventHandler(
            DiscordSocketClient client,
            BirthdayBackgroundService backgroundService,
            InteractionService commands)
        {
            _client = client;
            _backgroundService = backgroundService;
            _commands = commands;
        }

        public void Initialize()
        {
            _client.Ready += OnReady;
            _client.JoinedGuild += OnJoinedGuild;
        }

        // BOT READY
        private async Task OnReady()
        {
            Console.WriteLine("READY EVENT TRIGGERED");

            foreach (var guild in _client.Guilds)
            {
                await _commands.RegisterCommandsToGuildAsync(guild.Id);
                Console.WriteLine($"Commands registriert für {guild.Name}");
            }

            _backgroundService.Start();
        }

        // NEW SERVER
        private Task OnJoinedGuild(SocketGuild guild)
        {
            Console.WriteLine($"Joined guild: {guild.Name}");
            return Task.CompletedTask;
        }
    }
}