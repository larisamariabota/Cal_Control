using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using UniApp.Models;

namespace UniApp.Services
{
    public class SportService
    {
        public static bool AddSportLog(int userId, string sportName, int minutes, int caloriesBurned, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(sportName) || minutes <= 0 || caloriesBurned < 0) return false;

            using (var connection = DatabaseService.GetConnection())
            {
                var query = @"
                    INSERT INTO SportLogs (UserId, SportName, Minutes, CaloriesBurned, Date)
                    VALUES (@userId, @sportName, @minutes, @caloriesBurned, @date);";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@sportName", sportName);
                    command.Parameters.AddWithValue("@minutes", minutes);
                    command.Parameters.AddWithValue("@caloriesBurned", caloriesBurned);
                    command.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
                    command.ExecuteNonQuery();
                    JsonExportService.ExportAllData();
                    return true;
                }
            }
        }

        public static List<SportLog> GetLogsForDate(int userId, DateTime date)
        {
            return GetLogsBetween(userId, date, date);
        }

        public static List<SportLog> GetLogsBetween(int userId, DateTime startDate, DateTime endDate)
        {
            var logs = new List<SportLog>();
            using (var connection = DatabaseService.GetConnection())
            {
                var query = @"
                    SELECT Id, SportName, Minutes, CaloriesBurned, Date
                    FROM SportLogs
                    WHERE UserId = @userId AND Date >= @startDate AND Date <= @endDate
                    ORDER BY Id DESC;";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd"));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new SportLog
                            {
                                Id = reader.GetInt32(0),
                                UserId = userId,
                                SportName = reader.GetString(1),
                                Minutes = reader.GetInt32(2),
                                CaloriesBurned = reader.GetInt32(3),
                                Date = DateTime.Parse(reader.GetString(4))
                            });
                        }
                    }
                }
            }

            return logs;
        }
    }
}
