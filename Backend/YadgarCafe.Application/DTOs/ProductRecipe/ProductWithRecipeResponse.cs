using YadgarCafe.Application.DTOs.Product;

namespace YadgarCafe.Application.DTOs.ProductRecipe
{
    public class ProductWithRecipeResponse
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public IEnumerable<RecipeIngredientResponse> Ingredients { get; set; } = new List<RecipeIngredientResponse>();
    }

    public class RecipeIngredientResponse
    {
        public Guid IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal CurrentStock { get; set; }
        public bool IsAvailable => CurrentStock >= Quantity;
    }
}
