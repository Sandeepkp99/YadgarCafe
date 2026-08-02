namespace YadgarCafe.Application.DTOs.Product
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;
        public Guid CategoryId { get; set; }
    }
}
