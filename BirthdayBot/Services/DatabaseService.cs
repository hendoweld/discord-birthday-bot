using Npgsql;

namespace BirthdayBot.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly LoggingService _logger;

        public DatabaseService(string connectionString, LoggingService logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task<NpgsqlConnection> GetConnection()
        {
            var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            return conn;
        }
        public async Task Initialize()
        {
            using var conn = await GetConnection();

            _logger?.Info("DB Init gestartet");

            var cmd = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS birthdays (
                    guild_id BIGINT NOT NULL,
                    user_id BIGINT NOT NULL,
                    day INT NOT NULL,
                    month INT NOT NULL,
                    channel_id BIGINT NOT NULL,
                    last_notified TIMESTAMP,
                    PRIMARY KEY (guild_id, user_id)
                );

                CREATE TABLE IF NOT EXISTS guild_config (
                    guild_id BIGINT PRIMARY KEY,
                    birthday_channel_id BIGINT NOT NULL,
                    birthday_role_id BIGINT NOT NULL
                );

            ", conn);

            await cmd.ExecuteNonQueryAsync();

            _logger?.Info("DB Tabellen erfolgreich erstellt/überprüft");
        }
    }
}