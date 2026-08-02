namespace YadgarCafe.Application.DTOs.ProductRecipe
{
    public class CreateProductRecipeRequest
    {
        public Guid ProductId { get; set; }
        public Guid IngredientId { get; set; }
        public decimal Quantity { get; set; }
    }
}
