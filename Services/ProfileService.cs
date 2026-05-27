using Microsoft.Data.Sqlite;
using UniApp.Models;

namespace UniApp.Services
{
    public static class ProfileService
    {
        public static UserProfile GetOrCreateProfile(int userId)
        {
            using (var connection = DatabaseService.GetConnection())
            {
                using (var command = new SqliteCommand("SELECT Age, HeightCm, WeightKg, Sex, ActivityLevel, Goal, DailyCalorieGoal FROM UserProfiles WHERE UserId = @userId;", connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserProfile
                            {
                                UserId = userId,
                                Age = reader.GetInt32(0),
                                HeightCm = reader.GetInt32(1),
                                WeightKg = reader.GetDouble(2),
                                Sex = reader.GetString(3),
                                ActivityLevel = reader.GetString(4),
                                Goal = reader.GetString(5),
                                DailyCalorieGoal = reader.GetInt32(6)
                            };
                        }
                    }
                }
            }

            var profile = new UserProfile { UserId = userId };
            SaveProfile(profile);
            return profile;
        }

        public static void SaveProfile(UserProfile profile)
        {
            profile.DailyCalorieGoal = CalculateDailyGoal(profile);

            using (var connection = DatabaseService.GetConnection())
            using (var command = new SqliteCommand(@"
                INSERT INTO UserProfiles (UserId, Age, HeightCm, WeightKg, Sex, ActivityLevel, Goal, DailyCalorieGoal)
                VALUES (@userId, @age, @height, @weight, @sex, @activity, @goal, @dailyGoal)
                ON CONFLICT(UserId) DO UPDATE SET
                    Age = excluded.Age,
                    HeightCm = excluded.HeightCm,
                    WeightKg = excluded.WeightKg,
                    Sex = excluded.Sex,
                    ActivityLevel = excluded.ActivityLevel,
                    Goal = excluded.Goal,
                    DailyCalorieGoal = excluded.DailyCalorieGoal;", connection))
            {
                command.Parameters.AddWithValue("@userId", profile.UserId);
                command.Parameters.AddWithValue("@age", profile.Age);
                command.Parameters.AddWithValue("@height", profile.HeightCm);
                command.Parameters.AddWithValue("@weight", profile.WeightKg);
                command.Parameters.AddWithValue("@sex", profile.Sex);
                command.Parameters.AddWithValue("@activity", profile.ActivityLevel);
                command.Parameters.AddWithValue("@goal", profile.Goal);
                command.Parameters.AddWithValue("@dailyGoal", profile.DailyCalorieGoal);
                command.ExecuteNonQuery();
            }

            JsonExportService.ExportAllData();
        }

        public static int CalculateDailyGoal(UserProfile profile)
        {
            var bmr = profile.Sex == "M"
                ? 10 * profile.WeightKg + 6.25 * profile.HeightCm - 5 * profile.Age + 5
                : 10 * profile.WeightKg + 6.25 * profile.HeightCm - 5 * profile.Age - 161;

            var factor = profile.ActivityLevel switch
            {
                "Sedentar" => 1.2,
                "Usor" => 1.375,
                "Activ" => 1.725,
                _ => 1.55
            };

            var target = bmr * factor;
            if (profile.Goal == "Slabire") target -= 400;
            if (profile.Goal == "Masa musculara") target += 300;
            return (int)System.Math.Round(target);
        }
    }
}
