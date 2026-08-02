using Microsoft.EntityFrameworkCore;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Infrastructure.Persistence;

namespace YadgarCafe.Infrastructure.Repositories
{
    public class IngredientRepository : IIngredientRepository
    {
        private readonly ApplicationDbContext _context;

        public IngredientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ingredient>> GetAllAsync()
        {
            return await _context.Ingredients
                .Where(i => !i.IsDeleted)
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ingredient>> GetLowStockAsync()
        {
            return await _context.Ingredients
                .Where(i => !i.IsDeleted && i.CurrentStock <= i.MinimumStock)
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<Ingredient?> GetByIdAsync(Guid id)
        {
            return await _context.Ingredients
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        }

        public async Task<Ingredient> AddAsync(Ingredient ingredient)
        {
            ingredient.CreatedOn = DateTime.UtcNow;
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();
            return ingredient;
        }

        public async Task<Ingredient?> UpdateAsync(Ingredient ingredient)
        {
            var existingIngredient = await GetByIdAsync(ingredient.Id);
            if (existingIngredient == null)
                return null;

            existingIngredient.Name = ingredient.Name;
            existingIngredient.Unit = ingredient.Unit;
            existingIngredient.CurrentStock = ingredient.CurrentStock;
            existingIngredient.MinimumStock = ingredient.MinimumStock;
            existingIngredient.ModifiedOn = DateTime.UtcNow;

            _context.Ingredients.Update(existingIngredient);
            await _context.SaveChangesAsync();

            return existingIngredient;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var ingredient = await GetByIdAsync(id);
            if (ingredient == null)
                return false;

            ingredient.IsDeleted = true;
            ingredient.ModifiedOn = DateTime.UtcNow;

            _context.Ingredients.Update(ingredient);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Ingredients
                .AnyAsync(i => i.Name == name && !i.IsDeleted);
        }
    }
}
