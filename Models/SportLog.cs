using System;

namespace UniApp.Models
{
    public class SportLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string SportName { get; set; } = string.Empty;
        public int Minutes { get; set; }
        public int CaloriesBurned { get; set; }
        public DateTime Date { get; set; }

        public string Details => $"{Minutes} min • {CaloriesBurned} kcal arse";
    }
}
