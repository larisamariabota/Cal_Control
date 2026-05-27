using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using UniApp.Models;

namespace UniApp.Services
{
    public class AuthService
    {
        // Aceasta functie criptează parola ca să nu fie salvată în text clar
        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        // Funcția de Înregistrare
        public static bool Register(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            string hashedPassword = HashPassword(password);

            try
            {
                using (var connection = DatabaseService.GetConnection())
                {
                    var query = "INSERT INTO Users (Username, PasswordHash) VALUES (@username, @password);";
                    using (var command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username.Trim());
                        command.Parameters.AddWithValue("@password", hashedPassword);
                        command.ExecuteNonQuery();
                        JsonExportService.ExportAllData();
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false; // Probabil userul există deja
            }
        }

        // Aceasta este funcția LOGIN pe care o căutai în ViewModel!
        public static User? Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            string hashedPassword = HashPassword(password);
            User? user = null;

            try
            {
                using (var connection = DatabaseService.GetConnection())
                {
                    var query = "SELECT Id, Username FROM Users WHERE Username = @username AND PasswordHash = @password;";
                    using (var command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username.Trim());
                        command.Parameters.AddWithValue("@password", hashedPassword);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user = new User
                                {
                                    Id = reader.GetInt32(0),
                                    Username = reader.GetString(1)
                                };
                            }
                        }
                    }
                }

                if (user != null)
                {
                    DatabaseService.RecordLogin(user.Id);
                }
            }
            catch (Exception)
            {
                return null;
            }

            return user;
        }
    }
}
