using YadgarCafe.Application.DTOs.Inventory;
using YadgarCafe.Application.Services.Interfaces;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Infrastructure.Repositories;

namespace YadgarCafe.Infrastructure.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _repository;
        private readonly IIngredientService _ingredientService;

        public InventoryService(IInventoryRepository repository, IIngredientService ingredientService)
        {
            _repository = repository;
            _ingredientService = ingredientService;
        }

        public async Task<IEnumerable<InventoryResponse>> GetAllInventoryAsync()
        {
            var inventories = await _repository.GetAllAsync();
            return await MapToResponsesAsync(inventories);
        }

        public async Task<IEnumerable<InventoryResponse>> GetInventoryByIngredientAsync(Guid ingredientId)
        {
            var inventories = await _repository.GetByIngredientAsync(ingredientId);
            return await MapToResponsesAsync(inventories);
        }

        public async Task<IEnumerable<InventoryResponse>> GetLowStockInventoryAsync()
        {
            var inventories = await _repository.GetLowStockAsync();
            return await MapToResponsesAsync(inventories);
        }

        public async Task<InventoryResponse?> GetInventoryByIdAsync(Guid id)
        {
            var inventory = await _repository.GetByIdAsync(id);
            return inventory != null ? await MapToResponseAsync(inventory) : null;
        }

        public async Task<IEnumerable<InventoryHistoryResponse>> GetInventoryHistoryAsync(Guid ingredientId, int days = 30)
        {
            if (days <= 0)
                throw new ArgumentException("Days must be greater than 0.");

            var histories = await _repository.GetHistoryAsync(ingredientId, days);
            return histories.Select(MapToHistoryResponse);
        }

        public async Task<InventoryResponse> CreateInventoryAsync(CreateInventoryRequest request)
        {
            if (request.IngredientId == Guid.Empty)
                throw new ArgumentException("Ingredient ID is required.");

            if (request.Quantity < 0)
                throw new ArgumentException("Quantity cannot be negative.");

            if (!await _repository.IngredientExistsAsync(request.IngredientId))
                throw new InvalidOperationException($"Ingredient with ID '{request.IngredientId}' does not exist.");

            var inventory = new Inventory
            {
                IngredientId = request.IngredientId,
                Quantity = request.Quantity,
                StockDate = request.StockDate != DateTime.MinValue ? request.StockDate : DateTime.UtcNow,
                Remarks = request.Remarks ?? string.Empty,
                CreatedBy = "System",
                CreatedOn = DateTime.UtcNow
            };

            var created = await _repository.AddAsync(inventory);
            return await MapToResponseAsync(created);
        }

        public async Task<InventoryResponse?> UpdateInventoryAsync(Guid id, UpdateInventoryRequest request)
        {
            if (request.Quantity < 0)
                throw new ArgumentException("Quantity cannot be negative.");

            var existingInventory = await _repository.GetByIdAsync(id);
            if (existingInventory == null)
                return null;

            var inventory = new Inventory
            {
                Id = id,
                IngredientId = existingInventory.IngredientId,
                Quantity = request.Quantity,
                StockDate = request.StockDate != DateTime.MinValue ? request.StockDate : DateTime.UtcNow,
                Remarks = request.Remarks ?? string.Empty,
                ModifiedBy = "System"
            };

            var updated = await _repository.UpdateAsync(inventory);
            return updated != null ? await MapToResponseAsync(updated) : null;
        }

        public async Task<bool> DeleteInventoryAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        private async Task<InventoryResponse> MapToResponseAsync(Inventory inventory)
        {
            var ingredient = inventory.IngredientId != Guid.Empty
                ? await _ingredientService.GetIngredientByIdAsync(inventory.IngredientId)
                : null;

            return new InventoryResponse
            {
                Id = inventory.Id,
                IngredientId = inventory.IngredientId,
                Ingredient = ingredient,
                Quantity = inventory.Quantity,
                StockDate = inventory.StockDate,
                Remarks = inventory.Remarks,
                CreatedOn = inventory.CreatedOn,
                ModifiedOn = inventory.ModifiedOn
            };
        }

        private async Task<IEnumerable<InventoryResponse>> MapToResponsesAsync(IEnumerable<Inventory> inventories)
        {
            var responses = new List<InventoryResponse>();
            foreach (var inventory in inventories)
            {
                responses.Add(await MapToResponseAsync(inventory));
            }
            return responses;
        }

        private static InventoryHistoryResponse MapToHistoryResponse(Inventory inventory)
        {
            return new InventoryHistoryResponse
            {
                Id = inventory.Id,
                IngredientId = inventory.IngredientId,
                IngredientName = inventory.Ingredient?.Name ?? "Unknown",
                Quantity = inventory.Quantity,
                StockDate = inventory.StockDate,
                Remarks = inventory.Remarks,
                CreatedOn = inventory.CreatedOn
            };
        }
    }
}
