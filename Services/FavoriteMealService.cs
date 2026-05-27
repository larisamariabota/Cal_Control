using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using UniApp.Models;

namespace UniApp.Services
{
    public static class FavoriteMealService
    {
        public static List<FavoriteMeal> GetFavorites(int userId)
        {
            var favorites = new List<FavoriteMeal>();
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqliteCommand("SELECT Id, FoodName, Grams, CaloriesPer100g, MealType FROM FavoriteMeals WHERE UserId = @userId ORDER BY Id DESC;", connection))
            {
                command.Parameters.AddWithValue("@userId", userId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        favorites.Add(new FavoriteMeal
                        {
                            Id = reader.GetInt32(0),
                            UserId = userId,
                            FoodName = reader.GetString(1),
                            Grams = reader.GetInt32(2),
                            CaloriesPer100g = reader.GetInt32(3),
                            MealType = reader.GetString(4)
                        });
                    }
                }
            }

            return favorites;
        }

        public static bool AddFavorite(int userId, string foodName, int grams, int caloriesPer100g, string mealType)
        {
            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqliteCommand("INSERT INTO FavoriteMeals (UserId, FoodName, Grams, CaloriesPer100g, MealType) VALUES (@userId, @foodName, @grams, @caloriesPer100g, @mealType);", connection))
            {
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@foodName", foodName);
                command.Parameters.AddWithValue("@grams", grams);
                command.Parameters.AddWithValue("@caloriesPer100g", caloriesPer100g);
                command.Parameters.AddWithValue("@mealType", mealType);
                var success = command.ExecuteNonQuery() > 0;
                if (success) JsonExportService.ExportAllData();
                return success;
            }
        }
    }
}
