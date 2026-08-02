namespace YadgarCafe.Application.DTOs.Ingredient
{
    public class IngredientResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal MinimumStock { get; set; }
        public bool IsLowStock => CurrentStock <= MinimumStock;
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
