using System;

namespace UniApp.Models
{
    public class FoodLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public int Grams { get; set; }
        public int CaloriesPer100g { get; set; }
        public int Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }
        public string MealType { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        public string Details => $"{Grams} g | {MealType} | P {Protein:0.#}g C {Carbs:0.#}g F {Fat:0.#}g";
    }
}
