namespace YadgarCafe.Application.DTOs.Product
{
    public class UpdateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public Guid CategoryId { get; set; }
    }
}
