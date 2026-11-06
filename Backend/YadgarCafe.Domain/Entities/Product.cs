using YadgarCafe.Domain.Common;

namespace YadgarCafe.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public bool IsAvailable { get; set; }

        public Guid CategoryId { get; set; }

        public Category Category { get; set; } = null!;

        public ICollection<ProductRecipe> Recipes { get; set; } = new List<ProductRecipe>();
    }
}
