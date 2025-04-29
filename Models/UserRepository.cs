using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using healthmate_backend.Models;

namespace healthmate_backend.Database
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
            CreateUsersTableIfNotExists();
        }

        private void CreateUsersTableIfNotExists()
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            string query = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Name VARCHAR(100),
                    Email VARCHAR(100)
                );
            ";

            using var cmd = new MySqlCommand(query, conn);
            cmd.ExecuteNonQuery();
            Console.WriteLine("✅ Users table checked/created.");
        }

        public void CreateUser(User user)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            string query = "INSERT INTO Users (Name, Email) VALUES (@name, @email)";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name", user.Name);
            cmd.Parameters.AddWithValue("@email", user.Email);

            cmd.ExecuteNonQuery();
            Console.WriteLine("✅ User inserted.");
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            string query = "SELECT Id, Name, Email FROM Users";
            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                users.Add(new User
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    Email = reader.GetString("Email")
                });
            }

            return users;
        }

        public void UpdateUserEmail(int id, string newEmail)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            string query = "UPDATE Users SET Email = @email WHERE Id = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@email", newEmail);

            cmd.ExecuteNonQuery();
            Console.WriteLine("✅ User email updated.");
        }

        public void DeleteUser(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            string query = "DELETE FROM Users WHERE Id = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
            Console.WriteLine("✅ User deleted.");
        }
        public void PrintDatabaseSchema()
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            string query = @"
        SELECT TABLE_NAME, COLUMN_NAME, COLUMN_TYPE
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = 'defaultdb'
        ORDER BY TABLE_NAME, ORDINAL_POSITION;
    ";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            string currentTable = null;

            Console.WriteLine("📦 Database Schema:");
            while (reader.Read())
            {
                string table = reader.GetString("TABLE_NAME");
                string column = reader.GetString("COLUMN_NAME");
                string type = reader.GetString("COLUMN_TYPE");

                if (table != currentTable)
                {
                    currentTable = table;
                    Console.WriteLine($"\n🔸 Table: {table}");
                }

                Console.WriteLine($"   - {column} ({type})");
            }
        }
    }
}

