using BirthdayBot.Database.Models;
using BirthdayBot.Services;
using Npgsql;

namespace BirthdayBot.Database
{
    public class GuildRepository
    {
        private readonly DatabaseService _db;

        public GuildRepository(DatabaseService db)
        {
            _db = db;
        }

        public async Task<Guild?> GetGuild(ulong guildId)
        {
            await using var conn = await _db.GetConnection();

            var cmd = new NpgsqlCommand(
                @"SELECT guild_id, birthday_channel_id, birthday_role_id
                  FROM guild_config
                  WHERE guild_id = @g",
                conn);

            cmd.Parameters.AddWithValue("g", (long)guildId);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new Guild
            {
                GuildId = (ulong)reader.GetInt64(0),
                BirthdayChannelId = (ulong)reader.GetInt64(1),
                BirthdayRoleId = (ulong)reader.GetInt64(2)
            };
        }

        public async Task SaveGuild(Guild config)
        {
            await using var conn = await _db.GetConnection();

            var cmd = new NpgsqlCommand(
                @"INSERT INTO guild_config
                (guild_id, birthday_channel_id, birthday_role_id)
                VALUES (@g, @c, @r)
                ON CONFLICT (guild_id)
                DO UPDATE SET
                    birthday_channel_id = EXCLUDED.birthday_channel_id,
                    birthday_role_id = EXCLUDED.birthday_role_id",
                conn);

            cmd.Parameters.AddWithValue("g", (long)config.GuildId);
            cmd.Parameters.AddWithValue("c", (long)config.BirthdayChannelId);
            cmd.Parameters.AddWithValue("r", (long)config.BirthdayRoleId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}