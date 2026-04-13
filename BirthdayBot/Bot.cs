using Discord;
using Discord.WebSocket;
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

        public async Task RunAsync(string token)
        {
            _client = new DiscordSocketClient();

            _client.Log += Log;
            _client.Ready += Ready;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Ready()
        {
            Console.WriteLine($"Bot online als {_client.CurrentUser}");
            return Task.CompletedTask;
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
