using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using UniApp.Models;

namespace UniApp.Services
{
    public static class FoodCatalogService
    {
        public static List<FoodCatalogItem> LoadFoods()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "baza_date_alimente_200.json");
            if (!File.Exists(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "baza_date_alimente_200.json");
            }

            if (!File.Exists(path))
            {
                return new List<FoodCatalogItem>
                {
                    new FoodCatalogItem { Name = "Mar", Category = "fructe", CaloriesPer100g = 52 },
                    new FoodCatalogItem { Name = "Piept de pui", Category = "carne", CaloriesPer100g = 165 },
                    new FoodCatalogItem { Name = "Orez fiert", Category = "cereale", CaloriesPer100g = 130 }
                };
            }

            var foods = new List<FoodCatalogItem>();
            using (var document = JsonDocument.Parse(File.ReadAllText(path)))
            {
                if (!document.RootElement.TryGetProperty("foods", out var foodsElement))
                {
                    return foods;
                }

                foreach (var item in foodsElement.EnumerateArray())
                {
                    var name = item.GetProperty("name").GetString() ?? string.Empty;
                    var category = item.GetProperty("category").GetString() ?? string.Empty;
                    var nutrition = item.GetProperty("nutrition_per_100g");
                    var calories = nutrition.GetProperty("calories_kcal").GetInt32();

                    foods.Add(new FoodCatalogItem
                    {
                        Name = name,
                        Category = category,
                        CaloriesPer100g = calories,
                        ProteinPer100g = nutrition.GetProperty("protein_g").GetDouble(),
                        CarbsPer100g = nutrition.GetProperty("carbohydrates_g").GetDouble(),
                        FatPer100g = nutrition.GetProperty("fat_g").GetDouble()
                    });
                }
            }

            return foods;
        }
    }
}
