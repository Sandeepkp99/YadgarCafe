namespace YadgarCafe.Application.DTOs.Inventory
{
    public class UpdateInventoryRequest
    {
        public decimal Quantity { get; set; }
        public DateTime StockDate { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }
}
