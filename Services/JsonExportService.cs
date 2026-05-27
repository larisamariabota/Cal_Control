using System;
using System.IO;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace UniApp.Services
{
    public static class JsonExportService
    {
        private static readonly string ExportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tracker_export.json");

        public static string JsonExportPath => ExportPath;

        public static void ExportAllData()
        {
            try
            {
                using (var connection = DatabaseService.GetConnection())
                {
                    var export = new
                    {
                        exportedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        databasePath = DatabaseService.DatabasePath,
                        users = ReadUsers(connection),
                        userProfiles = ReadUserProfiles(connection),
                        loginHistory = ReadLoginHistory(connection),
                        foodLogs = ReadFoodLogs(connection),
                        sportLogs = ReadSportLogs(connection),
                        favoriteMeals = ReadFavoriteMeals(connection)
                    };

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };

                    File.WriteAllText(ExportPath, JsonSerializer.Serialize(export, options));
                }
            }
            catch
            {
            }
        }

        private static object[] ReadUsers(SqliteConnection connection)
        {
            using (var command = new SqliteCommand("SELECT Id, Username FROM Users ORDER BY Id;", connection))
            using (var reader = command.ExecuteReader())
            {
                var rows = new System.Collections.Generic.List<object>();
                while (reader.Read())
                {
                    rows.Add(new
                    {
                        id = reader.GetInt32(0),
                        username = reader.GetString(1)
                    });
                }

                return rows.ToArray();
            }
        }

        private static object[] ReadLoginHistory(SqliteConnection connection)
        {
            using (var command = new SqliteCommand("SELECT Id, UserId, LoggedInAt FROM LoginHistory ORDER BY Id;", connection))
            using (var reader = command.ExecuteReader())
            {
                var rows = new System.Collections.Generic.List<object>();
                while (reader.Read())
                {
                    rows.Add(new
                    {
                        id = reader.GetInt32(0),
                        userId = reader.GetInt32(1),
                        loggedInAt = reader.GetString(2)
                    });
                }

                return rows.ToArray();
            }
        }

        private static object[] ReadUserProfiles(SqliteConnection connection)
        {
            using (var command = new SqliteCommand("SELECT UserId, Age, HeightCm, WeightKg, Sex, ActivityLevel, Goal, DailyCalorieGoal FROM UserProfiles ORDER BY UserId;", connection))
            using (var reader = command.ExecuteReader())
            {
                var rows = new System.Collections.Generic.List<object>();
                while (reader.Read())
                {
                    rows.Add(new
                    {
                        userId = reader.GetInt32(0),
                        age = reader.GetInt32(1),
                        heightCm = reader.GetInt32(2),
                        weightKg = reader.GetDouble(3),
                        sex = reader.GetString(4),
                        activityLevel = reader.GetString(5),
                        goal = reader.GetString(6),
                        dailyCalorieGoal = reader.GetInt32(7)
                    });
                }

                return rows.ToArray();
            }
        }

        private static object[] ReadFoodLogs(SqliteConnection connection)
        {
            var query = @"
                SELECT Id, UserId, FoodName, Grams, CaloriesPer100g, Calories, Protein, Carbs, Fat, MealType, Date
                FROM FoodLogs
                ORDER BY Date DESC, Id DESC;";

            using (var command = new SqliteCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                var rows = new System.Collections.Generic.List<object>();
                while (reader.Read())
                {
                    rows.Add(new
                    {
                        id = reader.GetInt32(0),
                        userId = reader.GetInt32(1),
                        foodName = reader.GetString(2),
                        grams = reader.GetInt32(3),
                        caloriesPer100g = reader.GetInt32(4),
                        calories = reader.GetInt32(5),
                        protein = reader.GetDouble(6),
                        carbs = reader.GetDouble(7),
                        fat = reader.GetDouble(8),
                        mealType = reader.GetString(9),
                        date = reader.GetString(10)
                    });
                }

                return rows.ToArray();
            }
        }

        private static object[] ReadSportLogs(SqliteConnection connection)
        {
            var query = @"
                SELECT Id, UserId, SportName, Minutes, CaloriesBurned, Date
                FROM SportLogs
                ORDER BY Date DESC, Id DESC;";

            using (var command = new SqliteCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                var rows = new System.Collections.Generic.List<object>();
                while (reader.Read())
                {
                    rows.Add(new
                    {
                        id = reader.GetInt32(0),
                        userId = reader.GetInt32(1),
                        sportName = reader.GetString(2),
                        minutes = reader.GetInt32(3),
                        caloriesBurned = reader.GetInt32(4),
                        date = reader.GetString(5)
                    });
                }

                return rows.ToArray();
            }
        }

        private static object[] ReadFavoriteMeals(SqliteConnection connection)
        {
            using (var command = new SqliteCommand("SELECT Id, UserId, FoodName, Grams, CaloriesPer100g, MealType FROM FavoriteMeals ORDER BY Id;", connection))
            using (var reader = command.ExecuteReader())
            {
                var rows = new System.Collections.Generic.List<object>();
                while (reader.Read())
                {
                    rows.Add(new
                    {
                        id = reader.GetInt32(0),
                        userId = reader.GetInt32(1),
                        foodName = reader.GetString(2),
                        grams = reader.GetInt32(3),
                        caloriesPer100g = reader.GetInt32(4),
                        mealType = reader.GetString(5)
                    });
                }

                return rows.ToArray();
            }
        }
    }
}
