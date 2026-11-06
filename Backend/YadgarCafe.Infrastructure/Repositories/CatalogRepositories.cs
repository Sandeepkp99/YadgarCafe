using Microsoft.EntityFrameworkCore;
using YadgarCafe.Application.Common;
using YadgarCafe.Application.Interfaces;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Infrastructure.Persistence;

namespace YadgarCafe.Infrastructure.Repositories
{
    internal static class QueryExtensions
    {
        public static async Task<PagedResult<T>> ToPagedAsync<T>(
            this IQueryable<T> query, PageQuery page, CancellationToken ct)
        {
            var total = await query.CountAsync(ct);
            var items = await query.Skip(page.Skip).Take(page.PageSize).ToListAsync(ct);
            return new PagedResult<T>(items, page.Page, page.PageSize, total);
        }

        /// <summary>Escapes LIKE wildcards so user input is matched literally.</summary>
        public static string EscapeLike(string value) =>
            value.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db) => _db = db;

        public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default) =>
            await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);

        public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

        public Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default) =>
            _db.Categories.AnyAsync(c => c.Name == name && c.Id != excludeId, ct);

        public Task<bool> HasProductsAsync(Guid id, CancellationToken ct = default) =>
            _db.Products.AnyAsync(p => p.CategoryId == id, ct);

        public async Task AddAsync(Category category, CancellationToken ct = default) =>
            await _db.Categories.AddAsync(category, ct);

        public void Remove(Category category) => _db.Categories.Remove(category);
    }

    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) => _db = db;

        public Task<PagedResult<Product>> GetPagedAsync(
            PageQuery page, Guid? categoryId, string? search, CancellationToken ct = default)
        {
            var query = _db.Products.AsNoTracking().Include(p => p.Category).AsQueryable();

            if (categoryId is not null)
                query = query.Where(p => p.CategoryId == categoryId);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => EF.Functions.Like(p.Name, $"%{QueryExtensions.EscapeLike(search.Trim())}%"));

            return query.OrderBy(p => p.Name).ToPagedAsync(page, ct);
        }

        public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            var list = ids.ToList();
            return await _db.Products.Where(p => list.Contains(p.Id)).ToListAsync(ct);
        }

        public Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default) =>
            _db.Products.AnyAsync(p => p.Name == name && p.Id != excludeId, ct);

        public async Task AddAsync(Product product, CancellationToken ct = default) =>
            await _db.Products.AddAsync(product, ct);

        public void Remove(Product product) => _db.Products.Remove(product);
    }

    public class IngredientRepository : IIngredientRepository
    {
        private readonly ApplicationDbContext _db;

        public IngredientRepository(ApplicationDbContext db) => _db = db;

        public Task<PagedResult<Ingredient>> GetPagedAsync(PageQuery page, bool lowStockOnly, CancellationToken ct = default)
        {
            var query = _db.Ingredients.AsNoTracking().AsQueryable();

            if (lowStockOnly)
                query = query.Where(i => i.CurrentStock <= i.MinimumStock);

            return query.OrderBy(i => i.Name).ToPagedAsync(page, ct);
        }

        public Task<int> CountLowStockAsync(CancellationToken ct = default) =>
            _db.Ingredients.CountAsync(i => i.CurrentStock <= i.MinimumStock, ct);

        public Task<Ingredient?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            _db.Ingredients.FirstOrDefaultAsync(i => i.Id == id, ct);

        public Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default) =>
            _db.Ingredients.AnyAsync(i => i.Name == name && i.Id != excludeId, ct);

        public Task<bool> IsUsedInRecipesAsync(Guid id, CancellationToken ct = default) =>
            _db.ProductRecipes.AnyAsync(r => r.IngredientId == id, ct);

        public async Task AddAsync(Ingredient ingredient, CancellationToken ct = default) =>
            await _db.Ingredients.AddAsync(ingredient, ct);

        public void Remove(Ingredient ingredient) => _db.Ingredients.Remove(ingredient);
    }

    public class InventoryRepository : IInventoryRepository
    {
        private readonly ApplicationDbContext _db;

        public InventoryRepository(ApplicationDbContext db) => _db = db;

        public Task<PagedResult<Inventory>> GetPagedAsync(
            PageQuery page, Guid? ingredientId, DateTime? fromUtc, CancellationToken ct = default)
        {
            var query = _db.Inventories.AsNoTracking().Include(i => i.Ingredient).AsQueryable();

            if (ingredientId is not null)
                query = query.Where(i => i.IngredientId == ingredientId);

            if (fromUtc is not null)
                query = query.Where(i => i.StockDate >= fromUtc);

            return query.OrderByDescending(i => i.StockDate).ThenByDescending(i => i.CreatedOn).ToPagedAsync(page, ct);
        }

        public Task<Inventory?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            _db.Inventories.AsNoTracking().Include(i => i.Ingredient).FirstOrDefaultAsync(i => i.Id == id, ct);

        public async Task<IReadOnlyList<Inventory>> GetOrderConsumptionAsync(Guid orderId, CancellationToken ct = default) =>
            await _db.Inventories
                .Include(i => i.Ingredient)
                .Where(i => i.OrderId == orderId && i.Type == Domain.Enums.StockMovementType.OrderConsumption)
                .ToListAsync(ct);

        public async Task AddAsync(Inventory entry, CancellationToken ct = default) =>
            await _db.Inventories.AddAsync(entry, ct);

        public async Task AddRangeAsync(IEnumerable<Inventory> entries, CancellationToken ct = default) =>
            await _db.Inventories.AddRangeAsync(entries, ct);
    }

    public class ProductRecipeRepository : IProductRecipeRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRecipeRepository(ApplicationDbContext db) => _db = db;

        public async Task<IReadOnlyList<ProductRecipe>> GetAllAsync(CancellationToken ct = default) =>
            await _db.ProductRecipes.AsNoTracking()
                .Include(r => r.Product).Include(r => r.Ingredient)
                .OrderBy(r => r.Product.Name).ThenBy(r => r.Ingredient.Name)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<ProductRecipe>> GetByProductAsync(Guid productId, CancellationToken ct = default) =>
            await _db.ProductRecipes
                .Include(r => r.Product).Include(r => r.Ingredient)
                .Where(r => r.ProductId == productId)
                .OrderBy(r => r.Ingredient.Name)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<ProductRecipe>> GetByProductIdsAsync(IEnumerable<Guid> productIds, CancellationToken ct = default)
        {
            var ids = productIds.ToList();
            return await _db.ProductRecipes
                .Include(r => r.Ingredient)
                .Where(r => ids.Contains(r.ProductId))
                .ToListAsync(ct);
        }

        public Task<ProductRecipe?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            _db.ProductRecipes.Include(r => r.Product).Include(r => r.Ingredient)
                .FirstOrDefaultAsync(r => r.Id == id, ct);

        public Task<bool> ExistsAsync(Guid productId, Guid ingredientId, CancellationToken ct = default) =>
            _db.ProductRecipes.AnyAsync(r => r.ProductId == productId && r.IngredientId == ingredientId, ct);

        public async Task AddAsync(ProductRecipe recipe, CancellationToken ct = default) =>
            await _db.ProductRecipes.AddAsync(recipe, ct);

        public void Remove(ProductRecipe recipe) => _db.ProductRecipes.Remove(recipe);

        public void RemoveRange(IEnumerable<ProductRecipe> recipes) => _db.ProductRecipes.RemoveRange(recipes);
    }
}
