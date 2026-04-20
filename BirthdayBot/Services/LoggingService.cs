namespace BirthdayBot.Services
{
    public class LoggingService
    {
        public void Info(string msg)
            => Console.WriteLine($"[{DateTime.Now:HH:mm:ss:fff}] INFO: {msg}");

        public void Error(string msg)
            => Console.WriteLine($"[{DateTime.Now:HH:mm:ss:fff}] ERROR: {msg}");

        public void Warn(string msg)
            => Console.WriteLine($"[{DateTime.Now:HH:mm:ss:fff}] WARN: {msg}");
    }
}