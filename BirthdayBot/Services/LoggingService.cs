using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot.Services
{
    public class LoggingService
    {
        public LoggingService(DiscordSocketClient client)
        {
            client.Log += LogAsync;
        }

        private Task LogAsync(LogMessage message)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{message.Severity}] {message.Source}: {message.Message}");

            if (message.Exception != null)
            {
                Console.WriteLine(message.Exception);
            }

            return Task.CompletedTask;
        }
    }
}
