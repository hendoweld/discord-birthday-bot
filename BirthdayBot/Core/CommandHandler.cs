using BirthdayBot.Services;
using Discord.Interactions;
using Discord.WebSocket;

namespace BirthdayBot.Core
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly BirthdayService _birthdayService;
        private readonly IServiceProvider _services;

        public CommandHandler(
            DiscordSocketClient client,
            InteractionService commands,
            IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            // Interaction Handler
            _client.InteractionCreated += HandleInteraction;

            // Modules laden
            await _commands.AddModulesAsync(typeof(Bot).Assembly, _services);

            Console.WriteLine("Modules geladen:");
            foreach (var module in _commands.Modules)
                Console.WriteLine($" - {module.Name}");

            Console.WriteLine("CommandHandler ready");
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            Console.WriteLine($"Interaction: {interaction.Type}");

            var context = new SocketInteractionContext(_client, interaction);

            try
            {
                var result = await _commands.ExecuteCommandAsync(context, _services);

                if (!result.IsSuccess)
                    Console.WriteLine($"Command Error: {result.ErrorReason}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION: {ex}");
            }
        }
    }
}