using YadgarCafe.Application.DTOs.Inventory;

namespace YadgarCafe.Application.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryResponse>> GetAllInventoryAsync();
        Task<IEnumerable<InventoryResponse>> GetInventoryByIngredientAsync(Guid ingredientId);
        Task<IEnumerable<InventoryResponse>> GetLowStockInventoryAsync();
        Task<InventoryResponse?> GetInventoryByIdAsync(Guid id);
        Task<IEnumerable<InventoryHistoryResponse>> GetInventoryHistoryAsync(Guid ingredientId, int days = 30);
        Task<InventoryResponse> CreateInventoryAsync(CreateInventoryRequest request);
        Task<InventoryResponse?> UpdateInventoryAsync(Guid id, UpdateInventoryRequest request);
        Task<bool> DeleteInventoryAsync(Guid id);
    }
}
