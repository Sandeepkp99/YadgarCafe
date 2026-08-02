namespace YadgarCafe.Application.DTOs.Inventory
{
    public class CreateInventoryRequest
    {
        public Guid IngredientId { get; set; }
        public decimal Quantity { get; set; }
        public DateTime StockDate { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }
}
