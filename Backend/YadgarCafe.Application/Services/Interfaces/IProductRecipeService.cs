using YadgarCafe.Application.DTOs.ProductRecipe;

namespace YadgarCafe.Application.Services.Interfaces
{
    public interface IProductRecipeService
    {
        Task<IEnumerable<ProductRecipeResponse>> GetAllRecipesAsync();
        Task<IEnumerable<ProductRecipeResponse>> GetRecipeByProductAsync(Guid productId);
        Task<ProductRecipeResponse?> GetRecipeByIdAsync(Guid id);
        Task<ProductWithRecipeResponse?> GetProductWithRecipeAsync(Guid productId);
        Task<ProductRecipeResponse> CreateRecipeAsync(CreateProductRecipeRequest request);
        Task<ProductRecipeResponse?> UpdateRecipeAsync(Guid id, UpdateProductRecipeRequest request);
        Task<bool> DeleteRecipeAsync(Guid id);
        Task<bool> DeleteAllRecipesByProductAsync(Guid productId);
        Task<bool> CanProduceProductAsync(Guid productId);
    }
}
