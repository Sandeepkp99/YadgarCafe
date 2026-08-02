using YadgarCafe.Application.DTOs.Ingredient;

namespace YadgarCafe.Application.Services.Interfaces
{
    public interface IIngredientService
    {
        Task<IEnumerable<IngredientResponse>> GetAllIngredientsAsync();
        Task<IEnumerable<IngredientResponse>> GetLowStockIngredientsAsync();
        Task<IngredientResponse?> GetIngredientByIdAsync(Guid id);
        Task<IngredientResponse> CreateIngredientAsync(CreateIngredientRequest request);
        Task<IngredientResponse?> UpdateIngredientAsync(Guid id, UpdateIngredientRequest request);
        Task<IngredientResponse?> UpdateStockAsync(Guid id, decimal quantity);
        Task<bool> DeleteIngredientAsync(Guid id);
    }
}
