namespace UniApp.Models
{
    public class WeeklyStat
    {
        public string DayLabel { get; set; } = string.Empty;
        public int FoodCalories { get; set; }
        public int BurnedCalories { get; set; }
        public int NetCalories { get; set; }
        public double FoodBarWidth { get; set; }
        public double BurnedBarWidth { get; set; }
    }
}
