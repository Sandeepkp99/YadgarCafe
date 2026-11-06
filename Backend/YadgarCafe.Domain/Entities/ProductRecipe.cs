using YadgarCafe.Domain.Common;

namespace YadgarCafe.Domain.Entities
{
    /// <summary>Quantity of one ingredient consumed to make one unit of a product.</summary>
    public class ProductRecipe : BaseEntity, IHardDelete
    {
        public Guid ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public Guid IngredientId { get; set; }

        public Ingredient Ingredient { get; set; } = null!;

        public decimal Quantity { get; set; }
    }
}
