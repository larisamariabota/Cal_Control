namespace UniApp.Models
{
    public class FavoriteMeal
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public int Grams { get; set; }
        public int CaloriesPer100g { get; set; }
        public string MealType { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{FoodName} - {Grams}g ({MealType})";
        }
    }
}
