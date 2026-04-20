using BirthdayBot;
using BirthdayBot.Config;
using BirthdayBot.Services;

class Program
{
    static async Task Main()
    {
        var logger = new LoggingService();
        var configLoader = new ConfigLoader(logger);
        var config = configLoader.Load();

        var bot = new Bot();
        await bot.RunAsync(config.Token);
    }
}