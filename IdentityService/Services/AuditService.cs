using Dapper; // Dapper için
using IdentityService.Data;
using Microsoft.Data.SqlClient; // MSSQL için Microsoft.Data.SqlClient kullanılırdı
using Npgsql; // PostgreSQL için Npgsql kullanacağız

namespace IdentityService.Services
{
    public class AuditService : IAuditService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public AuditService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task LogAsync(AuditLog logEntry)
        {
            // Dapper ile veritabanına log kaydı
            using var connection = new NpgsqlConnection(_connectionString); // PostgreSQL için NpgsqlConnection
            await connection.OpenAsync();

            var sql = "INSERT INTO \"AuditLogs\" (\"Timestamp\", \"EventType\", \"Username\", \"Details\") VALUES (@Timestamp, @EventType, @Username, @Details)";
            await connection.ExecuteAsync(sql, logEntry);
        }
    }
}