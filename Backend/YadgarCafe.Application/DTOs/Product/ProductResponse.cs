using YadgarCafe.Application.DTOs.Category;

namespace YadgarCafe.Application.DTOs.Product
{
    public class ProductResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public Guid CategoryId { get; set; }
        public CategoryResponse? Category { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
