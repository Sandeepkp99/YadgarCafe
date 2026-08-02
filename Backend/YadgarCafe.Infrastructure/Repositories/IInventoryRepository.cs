using YadgarCafe.Domain.Entities;

namespace YadgarCafe.Infrastructure.Repositories
{
    public interface IInventoryRepository
    {
        Task<IEnumerable<Inventory>> GetAllAsync();
        Task<IEnumerable<Inventory>> GetByIngredientAsync(Guid ingredientId);
        Task<IEnumerable<Inventory>> GetLowStockAsync();
        Task<IEnumerable<Inventory>> GetHistoryAsync(Guid ingredientId, int days);
        Task<Inventory?> GetByIdAsync(Guid id);
        Task<Inventory> AddAsync(Inventory inventory);
        Task<Inventory?> UpdateAsync(Inventory inventory);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> IngredientExistsAsync(Guid ingredientId);
    }
}
