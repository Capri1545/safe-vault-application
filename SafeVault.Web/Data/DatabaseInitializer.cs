using System.Data;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace SafeVault.Web.Data
{
    public class DatabaseInitializer(IConfiguration configuration)
    {
        private readonly string _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found."
            );
        private readonly string _databaseName = "SafeVaultDb";

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
            string sqlFilePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data",
                "database.sql"
            );
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
            // Sanitize inputs before using them in the query
            username = Model.InputValidator.Sanitize(username);
            email = Model.InputValidator.Sanitize(email);
            if (!Model.InputValidator.IsValidEmail(email))
                throw new ArgumentException("Invalid email format.");

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
