using YadgarCafe.Domain.Entities;

namespace YadgarCafe.Infrastructure.Repositories
{
    public interface IIngredientRepository
    {
        Task<IEnumerable<Ingredient>> GetAllAsync();
        Task<IEnumerable<Ingredient>> GetLowStockAsync();
        Task<Ingredient?> GetByIdAsync(Guid id);
        Task<Ingredient> AddAsync(Ingredient ingredient);
        Task<Ingredient?> UpdateAsync(Ingredient ingredient);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsByNameAsync(string name);
    }
}
