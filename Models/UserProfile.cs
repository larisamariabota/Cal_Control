namespace UniApp.Models
{
    public class UserProfile
    {
        public int UserId { get; set; }
        public int Age { get; set; } = 25;
        public int HeightCm { get; set; } = 170;
        public double WeightKg { get; set; } = 70;
        public string Sex { get; set; } = "F";
        public string ActivityLevel { get; set; } = "Moderat";
        public string Goal { get; set; } = "Mentinere";
        public int DailyCalorieGoal { get; set; } = 2000;
    }
}
