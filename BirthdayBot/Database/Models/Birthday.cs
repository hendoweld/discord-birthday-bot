namespace BirthdayBot.Database.Models
{
    public class Birthday
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public ulong ChannelId { get; set; }
        public DateTime? LastNotified { get; set; }
    }
}
