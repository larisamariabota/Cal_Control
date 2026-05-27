using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace UniApp.Services
{
    public class DatabaseService
    {
        private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tracker.db");
        private static readonly string ConnectionString = $"Data Source={DbPath}";

        public static string DatabasePath => DbPath;
        public static string JsonExportPath => JsonExportService.JsonExportPath;

        public static void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var createUsersTable = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        PasswordHash TEXT NOT NULL
                    );";

                using (var command = new SqliteCommand(createUsersTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                var createFoodLogsTable = @"
                    CREATE TABLE IF NOT EXISTS FoodLogs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL,
                        FoodName TEXT NOT NULL,
                        Grams INTEGER NOT NULL DEFAULT 100,
                        CaloriesPer100g INTEGER NOT NULL DEFAULT 0,
                        Calories INTEGER NOT NULL,
                        Protein REAL NOT NULL DEFAULT 0,
                        Carbs REAL NOT NULL DEFAULT 0,
                        Fat REAL NOT NULL DEFAULT 0,
                        MealType TEXT NOT NULL,
                        Date TEXT NOT NULL,
                        FOREIGN KEY(UserId) REFERENCES Users(Id)
                    );";

                using (var command = new SqliteCommand(createFoodLogsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                AddColumnIfMissing(connection, "FoodLogs", "Grams", "INTEGER NOT NULL DEFAULT 100");
                AddColumnIfMissing(connection, "FoodLogs", "CaloriesPer100g", "INTEGER NOT NULL DEFAULT 0");
                AddColumnIfMissing(connection, "FoodLogs", "Protein", "REAL NOT NULL DEFAULT 0");
                AddColumnIfMissing(connection, "FoodLogs", "Carbs", "REAL NOT NULL DEFAULT 0");
                AddColumnIfMissing(connection, "FoodLogs", "Fat", "REAL NOT NULL DEFAULT 0");

                var createLoginHistoryTable = @"
                    CREATE TABLE IF NOT EXISTS LoginHistory (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL,
                        LoggedInAt TEXT NOT NULL,
                        FOREIGN KEY(UserId) REFERENCES Users(Id)
                    );";

                using (var command = new SqliteCommand(createLoginHistoryTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                var createSportLogsTable = @"
                    CREATE TABLE IF NOT EXISTS SportLogs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL,
                        SportName TEXT NOT NULL,
                        Minutes INTEGER NOT NULL,
                        CaloriesBurned INTEGER NOT NULL,
                        Date TEXT NOT NULL,
                        FOREIGN KEY(UserId) REFERENCES Users(Id)
                    );";

                using (var command = new SqliteCommand(createSportLogsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                var createProfilesTable = @"
                    CREATE TABLE IF NOT EXISTS UserProfiles (
                        UserId INTEGER PRIMARY KEY,
                        Age INTEGER NOT NULL,
                        HeightCm INTEGER NOT NULL,
                        WeightKg REAL NOT NULL,
                        Sex TEXT NOT NULL,
                        ActivityLevel TEXT NOT NULL,
                        Goal TEXT NOT NULL,
                        DailyCalorieGoal INTEGER NOT NULL,
                        FOREIGN KEY(UserId) REFERENCES Users(Id)
                    );";

                using (var command = new SqliteCommand(createProfilesTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                var createFavoriteMealsTable = @"
                    CREATE TABLE IF NOT EXISTS FavoriteMeals (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL,
                        FoodName TEXT NOT NULL,
                        Grams INTEGER NOT NULL,
                        CaloriesPer100g INTEGER NOT NULL,
                        MealType TEXT NOT NULL,
                        FOREIGN KEY(UserId) REFERENCES Users(Id)
                    );";

                using (var command = new SqliteCommand(createFavoriteMealsTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            JsonExportService.ExportAllData();
        }

        private static void AddColumnIfMissing(SqliteConnection connection, string table, string column, string definition)
        {
            try
            {
                using (var command = new SqliteCommand($"ALTER TABLE {table} ADD COLUMN {column} {definition};", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (SqliteException)
            {
            }
        }

        public static void RecordLogin(int userId)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    var query = "INSERT INTO LoginHistory (UserId, LoggedInAt) VALUES (@userId, @loggedInAt);";
                    using (var command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        command.Parameters.AddWithValue("@loggedInAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.ExecuteNonQuery();
                    }
                }

                JsonExportService.ExportAllData();
            }
            catch (SqliteException)
            {
            }
        }

        public static SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            return connection;
        }
    }
}
