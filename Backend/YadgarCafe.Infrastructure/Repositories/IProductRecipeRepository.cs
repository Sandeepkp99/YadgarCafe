using YadgarCafe.Domain.Entities;

namespace YadgarCafe.Infrastructure.Repositories
{
    public interface IProductRecipeRepository
    {
        Task<IEnumerable<ProductRecipe>> GetAllAsync();
        Task<IEnumerable<ProductRecipe>> GetByProductAsync(Guid productId);
        Task<ProductRecipe?> GetByIdAsync(Guid id);
        Task<ProductRecipe?> GetByProductAndIngredientAsync(Guid productId, Guid ingredientId);
        Task<ProductRecipe> AddAsync(ProductRecipe recipe);
        Task<ProductRecipe?> UpdateAsync(ProductRecipe recipe);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeleteAllByProductAsync(Guid productId);
        Task<bool> ProductExistsAsync(Guid productId);
        Task<bool> IngredientExistsAsync(Guid ingredientId);
        Task<bool> RecipeExistsAsync(Guid productId, Guid ingredientId);
    }
}
