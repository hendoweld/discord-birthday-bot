using BirthdayBot.Database.Models;
using Npgsql;

namespace BirthdayBot.Database
{
    public class BirthdayRepository
    {
        private readonly DatabaseService _db;

        public BirthdayRepository(DatabaseService db)
        {
            _db = db;
        }

        public async Task<Birthday?> GetBirthday(ulong guildId, ulong userId)
        {
            await using var conn = await _db.GetConnection();

            var cmd = new NpgsqlCommand(
                @"SELECT guild_id, user_id, day, month, channel_id, last_notified
                  FROM birthdays
                  WHERE guild_id = @g AND user_id = @u",
                conn);

            cmd.Parameters.AddWithValue("g", (long)guildId);
            cmd.Parameters.AddWithValue("u", (long)userId);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new Birthday
            {
                GuildId = (ulong)reader.GetInt64(0),
                UserId = (ulong)reader.GetInt64(1),
                Day = reader.GetInt32(2),
                Month = reader.GetInt32(3),
                ChannelId = (ulong)reader.GetInt64(4),
                LastNotified = reader.IsDBNull(5)
                    ? null
                    : reader.GetDateTime(5)
            };
        }

        public async Task AddBirthday(Birthday birthday)
        {
            await using var conn = await _db.GetConnection();

            var cmd = new NpgsqlCommand(
                @"INSERT INTO birthdays
                (guild_id, user_id, day, month, channel_id)
                VALUES (@g, @u, @d, @m, @c)
                ON CONFLICT (guild_id, user_id)
                DO UPDATE SET
                    day = EXCLUDED.day,
                    month = EXCLUDED.month,
                    channel_id = EXCLUDED.channel_id",
                conn);

            cmd.Parameters.AddWithValue("g", (long)birthday.GuildId);
            cmd.Parameters.AddWithValue("u", (long)birthday.UserId);
            cmd.Parameters.AddWithValue("d", birthday.Day);
            cmd.Parameters.AddWithValue("m", birthday.Month);
            cmd.Parameters.AddWithValue("c", (long)birthday.ChannelId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}