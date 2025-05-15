using System.Data;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace SafeVault.Api.Data
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;
        private readonly string _databaseName = "SafeVaultDb";

        public DatabaseInitializer(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found."
                );
        }

        public void Initialize()
        {
            EnsureDatabaseExists();
            EnsureUsersTableExists();
        }

        private void EnsureDatabaseExists()
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            var initialCatalog = builder.InitialCatalog;
            builder.InitialCatalog = "master";
            using var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText =
                $"IF DB_ID('{_databaseName}') IS NULL CREATE DATABASE [{_databaseName}]";
            command.ExecuteNonQuery();
        }

        private void EnsureUsersTableExists()
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            using var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
            // Look for database.sql in the project source directory, not the bin directory
            string sqlFilePath = Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "Data",
                "database.sql"
            );
            sqlFilePath = Path.GetFullPath(sqlFilePath);
            if (!File.Exists(sqlFilePath))
                throw new FileNotFoundException($"Could not find database.sql at {sqlFilePath}");
            string createTableSql = File.ReadAllText(sqlFilePath);
            using var command = connection.CreateCommand();
            command.CommandText =
                "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')\nBEGIN\n"
                + createTableSql
                + "\nEND";
            command.ExecuteNonQuery();
        }

        public void AddUser(string username, string email)
        {
            // TODO: Add input validation or sanitization if needed
            var builder = new SqlConnectionStringBuilder(_connectionString);
            using var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Users (Username, Email) VALUES (@Username, @Email)";
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Email", email);
            command.ExecuteNonQuery();
        }
    }
}
