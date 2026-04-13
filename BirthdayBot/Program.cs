using BirthdayBot;
using BirthdayBot.Config;
using Discord;
using Discord.WebSocket;
using System.Text.Json;

class Program
{
    static async Task Main()
    {
        var config = ConfigLoader.Load();

        var bot = new Bot();
        await bot.RunAsync(config.Token);
    }
}