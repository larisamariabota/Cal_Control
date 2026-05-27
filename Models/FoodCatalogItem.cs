namespace UniApp.Models
{
    public class FoodCatalogItem
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int CaloriesPer100g { get; set; }
        public double ProteinPer100g { get; set; }
        public double CarbsPer100g { get; set; }
        public double FatPer100g { get; set; }

        public override string ToString()
        {
            return $"{Name} ({CaloriesPer100g} kcal/100g, P {ProteinPer100g:0.#}g)";
        }
    }
}
