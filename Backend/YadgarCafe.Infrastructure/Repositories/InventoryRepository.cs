using Microsoft.EntityFrameworkCore;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Infrastructure.Persistence;

namespace YadgarCafe.Infrastructure.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly ApplicationDbContext _context;

        public InventoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Inventory>> GetAllAsync()
        {
            return await _context.Inventories
                .Include(i => i.Ingredient)
                .Where(i => !i.IsDeleted)
                .OrderByDescending(i => i.StockDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> GetByIngredientAsync(Guid ingredientId)
        {
            return await _context.Inventories
                .Include(i => i.Ingredient)
                .Where(i => i.IngredientId == ingredientId && !i.IsDeleted)
                .OrderByDescending(i => i.StockDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> GetLowStockAsync()
        {
            return await _context.Inventories
                .Include(i => i.Ingredient)
                .Where(i => !i.IsDeleted && i.Quantity <= i.Ingredient.MinimumStock)
                .OrderByDescending(i => i.StockDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> GetHistoryAsync(Guid ingredientId, int days)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            return await _context.Inventories
                .Include(i => i.Ingredient)
                .Where(i => i.IngredientId == ingredientId && !i.IsDeleted && i.StockDate >= startDate)
                .OrderByDescending(i => i.StockDate)
                .ToListAsync();
        }

        public async Task<Inventory?> GetByIdAsync(Guid id)
        {
            return await _context.Inventories
                .Include(i => i.Ingredient)
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        }

        public async Task<Inventory> AddAsync(Inventory inventory)
        {
            inventory.CreatedOn = DateTime.UtcNow;
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task<Inventory?> UpdateAsync(Inventory inventory)
        {
            var existingInventory = await GetByIdAsync(inventory.Id);
            if (existingInventory == null)
                return null;

            existingInventory.Quantity = inventory.Quantity;
            existingInventory.StockDate = inventory.StockDate;
            existingInventory.Remarks = inventory.Remarks;
            existingInventory.ModifiedOn = DateTime.UtcNow;

            _context.Inventories.Update(existingInventory);
            await _context.SaveChangesAsync();

            return existingInventory;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var inventory = await GetByIdAsync(id);
            if (inventory == null)
                return false;

            inventory.IsDeleted = true;
            inventory.ModifiedOn = DateTime.UtcNow;

            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IngredientExistsAsync(Guid ingredientId)
        {
            return await _context.Ingredients
                .AnyAsync(i => i.Id == ingredientId && !i.IsDeleted);
        }
    }
}
