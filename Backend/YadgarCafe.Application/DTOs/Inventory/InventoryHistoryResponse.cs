using YadgarCafe.Application.DTOs.Ingredient;

namespace YadgarCafe.Application.DTOs.Inventory
{
    public class InventoryHistoryResponse
    {
        public Guid Id { get; set; }
        public Guid IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public DateTime StockDate { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
    }
}
