using Npgsql;

namespace DirectDbWebApp.Services {
    public class DataService {
        private readonly string _connectionString;

        public DataService(string connectionString) {

            this._connectionString = connectionString;
        }

        public async Task<NpgsqlDataReader> ExecuteQuery(string Query) {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(Query, conn);
            var reader = await cmd.ExecuteReaderAsync();

            return reader;

        }

    }
}
