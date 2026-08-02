namespace YadgarCafe.Application.DTOs.Ingredient
{
    public class UpdateIngredientRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal MinimumStock { get; set; }
    }
}
