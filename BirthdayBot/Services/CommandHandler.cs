using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
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
            _client.InteractionCreated += HandleInteraction;

            await _commands.AddModulesAsync(
                Assembly.GetEntryAssembly(),
                _services);

            await _commands.RegisterCommandsGloballyAsync();
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_client, interaction);

                await _commands.ExecuteCommandAsync(
                    context,
                    _services);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
