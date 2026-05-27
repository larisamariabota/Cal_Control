using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using UniApp.Models;

namespace UniApp.Services
{
    public class FoodService
    {
        // Salvează o masă nouă
        public static bool AddFoodLog(int userId, string foodName, int grams, int caloriesPer100g, int calories, double protein, double carbs, double fat, string mealType, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(foodName) || grams <= 0 || calories < 0) return false;

            using (var connection = DatabaseService.GetConnection())
            {
                var query = @"
                    INSERT INTO FoodLogs (UserId, FoodName, Grams, CaloriesPer100g, Calories, Protein, Carbs, Fat, MealType, Date)
                    VALUES (@userId, @foodName, @grams, @caloriesPer100g, @calories, @protein, @carbs, @fat, @mealType, @date);";
                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@foodName", foodName.Trim());
                    command.Parameters.AddWithValue("@grams", grams);
                    command.Parameters.AddWithValue("@caloriesPer100g", caloriesPer100g);
                    command.Parameters.AddWithValue("@calories", calories);
                    command.Parameters.AddWithValue("@protein", protein);
                    command.Parameters.AddWithValue("@carbs", carbs);
                    command.Parameters.AddWithValue("@fat", fat);
                    command.Parameters.AddWithValue("@mealType", mealType);
                    command.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
                    
                    command.ExecuteNonQuery();
                    JsonExportService.ExportAllData();
                    return true;
                }
            }
        }

        // Obține toate mesele unui utilizator pentru ziua de azi
        public static List<FoodLog> GetLogsForDate(int userId, DateTime date)
        {
            var logs = new List<FoodLog>();
            using (var connection = DatabaseService.GetConnection())
            {
                var query = @"
                    SELECT Id, FoodName, Grams, CaloriesPer100g, Calories, Protein, Carbs, Fat, MealType, Date
                    FROM FoodLogs
                    WHERE UserId = @userId AND Date = @date
                    ORDER BY Id DESC;";
                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new FoodLog
                            {
                                Id = reader.GetInt32(0),
                                UserId = userId,
                                FoodName = reader.GetString(1),
                                Grams = reader.GetInt32(2),
                                CaloriesPer100g = reader.GetInt32(3),
                                Calories = reader.GetInt32(4),
                                Protein = reader.GetDouble(5),
                                Carbs = reader.GetDouble(6),
                                Fat = reader.GetDouble(7),
                                MealType = reader.GetString(8),
                                Date = DateTime.Parse(reader.GetString(9))
                            });
                        }
                    }
                }
            }
            return logs;
        }

        public static List<FoodLog> GetLogsBetween(int userId, DateTime startDate, DateTime endDate)
        {
            var logs = new List<FoodLog>();
            using (var connection = DatabaseService.GetConnection())
            {
                var query = @"
                    SELECT Id, FoodName, Grams, CaloriesPer100g, Calories, Protein, Carbs, Fat, MealType, Date
                    FROM FoodLogs
                    WHERE UserId = @userId AND Date >= @startDate AND Date <= @endDate;";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd"));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new FoodLog
                            {
                                Id = reader.GetInt32(0),
                                UserId = userId,
                                FoodName = reader.GetString(1),
                                Grams = reader.GetInt32(2),
                                CaloriesPer100g = reader.GetInt32(3),
                                Calories = reader.GetInt32(4),
                                Protein = reader.GetDouble(5),
                                Carbs = reader.GetDouble(6),
                                Fat = reader.GetDouble(7),
                                MealType = reader.GetString(8),
                                Date = DateTime.Parse(reader.GetString(9))
                            });
                        }
                    }
                }
            }

            return logs;
        }

        public static bool UpdateFoodLog(int userId, int logId, string foodName, int grams, int caloriesPer100g, int calories, double protein, double carbs, double fat, string mealType, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(foodName) || grams <= 0 || calories < 0) return false;

            using (var connection = DatabaseService.GetConnection())
            {
                var query = @"
                    UPDATE FoodLogs
                    SET FoodName = @foodName,
                        Grams = @grams,
                        CaloriesPer100g = @caloriesPer100g,
                        Calories = @calories,
                        Protein = @protein,
                        Carbs = @carbs,
                        Fat = @fat,
                        MealType = @mealType,
                        Date = @date
                    WHERE Id = @id AND UserId = @userId;";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", logId);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@foodName", foodName.Trim());
                    command.Parameters.AddWithValue("@grams", grams);
                    command.Parameters.AddWithValue("@caloriesPer100g", caloriesPer100g);
                    command.Parameters.AddWithValue("@calories", calories);
                    command.Parameters.AddWithValue("@protein", protein);
                    command.Parameters.AddWithValue("@carbs", carbs);
                    command.Parameters.AddWithValue("@fat", fat);
                    command.Parameters.AddWithValue("@mealType", mealType);
                    command.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
                    var success = command.ExecuteNonQuery() > 0;
                    if (success)
                    {
                        JsonExportService.ExportAllData();
                    }

                    return success;
                }
            }
        }

        public static bool DeleteFoodLog(int userId, int logId)
        {
            using (var connection = DatabaseService.GetConnection())
            {
                var query = "DELETE FROM FoodLogs WHERE Id = @id AND UserId = @userId;";
                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", logId);
                    command.Parameters.AddWithValue("@userId", userId);
                    var success = command.ExecuteNonQuery() > 0;
                    if (success)
                    {
                        JsonExportService.ExportAllData();
                    }

                    return success;
                }
            }
        }
    }
}
