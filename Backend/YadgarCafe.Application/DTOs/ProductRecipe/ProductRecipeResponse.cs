using YadgarCafe.Application.DTOs.Ingredient;
using YadgarCafe.Application.DTOs.Product;

namespace YadgarCafe.Application.DTOs.ProductRecipe
{
    public class ProductRecipeResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public ProductResponse? Product { get; set; }
        public Guid IngredientId { get; set; }
        public IngredientResponse? Ingredient { get; set; }
        public decimal Quantity { get; set; }
    }
}
