using BirthdayBot.Services;
using Discord.Interactions;
using Discord.WebSocket;

namespace BirthdayBot.Core
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;
        private readonly LoggingService _logger;

        public CommandHandler(
            DiscordSocketClient client,
            InteractionService commands,
            IServiceProvider services,
            LoggingService logger)
        {
            _client = client;
            _commands = commands;
            _services = services;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            // Interaction Handler
            _client.InteractionCreated += HandleInteraction;

            // Modules laden
            await _commands.AddModulesAsync(typeof(Bot).Assembly, _services);

            _logger.Info("Modules geladen:");
            foreach (var module in _commands.Modules)
                _logger.Info($" - {module.Name}");

            _logger.Info("CommandHandler ready");
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            _logger.Info($"Interaction: {interaction.Type}");

            var context = new SocketInteractionContext(_client, interaction);

            try
            {
                var result = await _commands.ExecuteCommandAsync(context, _services);

                if (!result.IsSuccess)
                    _logger.Error($"Command Error: {result.ErrorReason}");
            }
            catch (Exception ex)
            {
                _logger.Error($"EXCEPTION: {ex}");
            }
        }
    }
}