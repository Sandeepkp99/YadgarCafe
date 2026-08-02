using Microsoft.EntityFrameworkCore;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Infrastructure.Persistence;

namespace YadgarCafe.Infrastructure.Repositories
{
    public class ProductRecipeRepository : IProductRecipeRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRecipeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductRecipe>> GetAllAsync()
        {
            return await _context.ProductRecipes
                .Include(pr => pr.Product)
                .Include(pr => pr.Ingredient)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductRecipe>> GetByProductAsync(Guid productId)
        {
            return await _context.ProductRecipes
                .Include(pr => pr.Product)
                .Include(pr => pr.Ingredient)
                .Where(pr => pr.ProductId == productId)
                .ToListAsync();
        }

        public async Task<ProductRecipe?> GetByIdAsync(Guid id)
        {
            return await _context.ProductRecipes
                .Include(pr => pr.Product)
                .Include(pr => pr.Ingredient)
                .FirstOrDefaultAsync(pr => pr.Id == id);
        }

        public async Task<ProductRecipe?> GetByProductAndIngredientAsync(Guid productId, Guid ingredientId)
        {
            return await _context.ProductRecipes
                .Include(pr => pr.Product)
                .Include(pr => pr.Ingredient)
                .FirstOrDefaultAsync(pr => pr.ProductId == productId && pr.IngredientId == ingredientId);
        }

        public async Task<ProductRecipe> AddAsync(ProductRecipe recipe)
        {
            _context.ProductRecipes.Add(recipe);
            await _context.SaveChangesAsync();
            return recipe;
        }

        public async Task<ProductRecipe?> UpdateAsync(ProductRecipe recipe)
        {
            var existingRecipe = await GetByIdAsync(recipe.Id);
            if (existingRecipe == null)
                return null;

            existingRecipe.Quantity = recipe.Quantity;

            _context.ProductRecipes.Update(existingRecipe);
            await _context.SaveChangesAsync();

            return existingRecipe;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var recipe = await GetByIdAsync(id);
            if (recipe == null)
                return false;

            _context.ProductRecipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAllByProductAsync(Guid productId)
        {
            var recipes = await GetByProductAsync(productId);
            if (!recipes.Any())
                return false;

            _context.ProductRecipes.RemoveRange(recipes);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ProductExistsAsync(Guid productId)
        {
            return await _context.Products
                .AnyAsync(p => p.Id == productId && !p.IsDeleted);
        }

        public async Task<bool> IngredientExistsAsync(Guid ingredientId)
        {
            return await _context.Ingredients
                .AnyAsync(i => i.Id == ingredientId && !i.IsDeleted);
        }

        public async Task<bool> RecipeExistsAsync(Guid productId, Guid ingredientId)
        {
            return await _context.ProductRecipes
                .AnyAsync(pr => pr.ProductId == productId && pr.IngredientId == ingredientId);
        }
    }
}
