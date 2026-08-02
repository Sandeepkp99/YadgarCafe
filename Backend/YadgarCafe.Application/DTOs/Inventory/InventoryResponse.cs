using YadgarCafe.Application.DTOs.Ingredient;

namespace YadgarCafe.Application.DTOs.Inventory
{
    public class InventoryResponse
    {
        public Guid Id { get; set; }
        public Guid IngredientId { get; set; }
        public IngredientResponse? Ingredient { get; set; }
        public decimal Quantity { get; set; }
        public DateTime StockDate { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public bool IsLowStock => Ingredient != null && Quantity <= Ingredient.MinimumStock;
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
