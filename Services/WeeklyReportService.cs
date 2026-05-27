using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace UniApp.Services
{
    public static class WeeklyReportService
    {
        public static string ExportWeeklyReport(int userId, DateTime endDate)
        {
            var startDate = endDate.Date.AddDays(-6);
            var foods = FoodService.GetLogsBetween(userId, startDate, endDate.Date);
            var sports = SportService.GetLogsBetween(userId, startDate, endDate.Date);
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"weekly_report_{endDate:yyyyMMdd}.json");

            var report = new
            {
                startDate = startDate.ToString("yyyy-MM-dd"),
                endDate = endDate.ToString("yyyy-MM-dd"),
                totalFoodCalories = foods.Sum(f => f.Calories),
                totalBurnedCalories = sports.Sum(s => s.CaloriesBurned),
                netCalories = foods.Sum(f => f.Calories) - sports.Sum(s => s.CaloriesBurned),
                protein = foods.Sum(f => f.Protein),
                carbs = foods.Sum(f => f.Carbs),
                fat = foods.Sum(f => f.Fat),
                bestSportDay = sports
                    .GroupBy(s => s.Date.Date)
                    .OrderByDescending(g => g.Sum(s => s.CaloriesBurned))
                    .Select(g => new { date = g.Key.ToString("yyyy-MM-dd"), calories = g.Sum(s => s.CaloriesBurned) })
                    .FirstOrDefault(),
                highestFoodDay = foods
                    .GroupBy(f => f.Date.Date)
                    .OrderByDescending(g => g.Sum(f => f.Calories))
                    .Select(g => new { date = g.Key.ToString("yyyy-MM-dd"), calories = g.Sum(f => f.Calories) })
                    .FirstOrDefault()
            };

            File.WriteAllText(path, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
            return path;
        }
    }
}
