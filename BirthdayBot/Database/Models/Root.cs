namespace BirthdayBot.Database.Models
{
    public class Root
    {
        public List<Birthday> Birthdays { get; set; } = new();
        public List<Guild> Guilds { get; set; } = new();
    }
}
