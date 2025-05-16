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
            EnsureDefaultAdminUser();
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

        private void EnsureDefaultAdminUser()
        {
            var admin = GetUserByUsername("admin");
            if (admin == null)
            {
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword("admin");
                AddUser("admin", hashedPassword, "Admin", "");
            }
        }

        public class DbUser
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Role { get; set; } = "User";
            public string Email { get; set; } = string.Empty;
        }

        public DbUser? GetUserByUsername(string username)
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            using var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT Username, Password, Role, Email FROM Users WHERE Username = @Username";
            command.Parameters.AddWithValue("@Username", username);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new DbUser
                {
                    Username = reader["Username"] as string ?? string.Empty,
                    Password = reader["Password"] as string ?? string.Empty,
                    Role = reader["Role"] as string ?? "User",
                    Email = reader["Email"] as string ?? string.Empty,
                };
            }
            return null;
        }

        public List<DbUser> GetAllUsers()
        {
            var users = new List<DbUser>();
            var builder = new SqlConnectionStringBuilder(_connectionString);
            using var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Username, Password, Role, Email FROM Users";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(
                    new DbUser
                    {
                        Username = reader["Username"] as string ?? string.Empty,
                        Password = reader["Password"] as string ?? string.Empty,
                        Role = reader["Role"] as string ?? "User",
                        Email = reader["Email"] as string ?? string.Empty,
                    }
                );
            }
            return users;
        }

        public void AddUser(string username, string password, string role, string email)
        {
            // Use shared InputValidator for all input sanitization
            username = SafeVault.Common.InputValidator.Sanitize(username);
            email = SafeVault.Common.InputValidator.Sanitize(email);
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Username and password are required.");
            if (
                !string.IsNullOrEmpty(email) && !SafeVault.Common.InputValidator.IsValidEmail(email)
            )
                throw new ArgumentException("Invalid email address.");
            var builder = new SqlConnectionStringBuilder(_connectionString);
            using var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText =
                "INSERT INTO Users (Username, Password, Role, Email) VALUES (@Username, @Password, @Role, @Email)";
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Password", password);
            command.Parameters.AddWithValue("@Role", role);
            command.Parameters.AddWithValue("@Email", email);
            command.ExecuteNonQuery();
        }

        public void DeleteUser(string username)
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            using var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Users WHERE Username = @Username";
            command.Parameters.AddWithValue("@Username", username);
            command.ExecuteNonQuery();
        }
    }
}
