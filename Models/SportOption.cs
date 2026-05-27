namespace UniApp.Models
{
    public class SportOption
    {
        public string Name { get; set; } = string.Empty;
        public int CaloriesPerHour { get; set; }

        public override string ToString()
        {
            return $"{Name} ({CaloriesPerHour} kcal/ora)";
        }
    }
}
