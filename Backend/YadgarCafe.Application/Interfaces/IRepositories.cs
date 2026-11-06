using YadgarCafe.Application.Common;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Domain.Enums;

namespace YadgarCafe.Application.Interfaces
{
    // Repositories only stage changes; IUnitOfWork persists them.
    // Remove(...) is a soft delete (see ApplicationDbContext.SaveChangesAsync) except for IHardDelete entities.

    public interface ICategoryRepository
    {
        Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default);
        Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
        Task<bool> HasProductsAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(Category category, CancellationToken ct = default);
        void Remove(Category category);
    }

    public interface IProductRepository
    {
        Task<PagedResult<Product>> GetPagedAsync(PageQuery page, Guid? categoryId, string? search, CancellationToken ct = default);
        Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
        Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
        Task AddAsync(Product product, CancellationToken ct = default);
        void Remove(Product product);
    }

    public interface IIngredientRepository
    {
        Task<PagedResult<Ingredient>> GetPagedAsync(PageQuery page, bool lowStockOnly, CancellationToken ct = default);
        Task<int> CountLowStockAsync(CancellationToken ct = default);
        Task<Ingredient?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
        Task<bool> IsUsedInRecipesAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(Ingredient ingredient, CancellationToken ct = default);
        void Remove(Ingredient ingredient);
    }

    public interface IInventoryRepository
    {
        Task<PagedResult<Inventory>> GetPagedAsync(PageQuery page, Guid? ingredientId, DateTime? fromUtc, CancellationToken ct = default);
        Task<Inventory?> GetByIdAsync(Guid id, CancellationToken ct = default);

        /// <summary>Ledger rows that consumed stock for an order, with tracked ingredients, so they can be reversed.</summary>
        Task<IReadOnlyList<Inventory>> GetOrderConsumptionAsync(Guid orderId, CancellationToken ct = default);

        Task AddAsync(Inventory entry, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<Inventory> entries, CancellationToken ct = default);
    }

    public interface IProductRecipeRepository
    {
        Task<IReadOnlyList<ProductRecipe>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<ProductRecipe>> GetByProductAsync(Guid productId, CancellationToken ct = default);

        /// <summary>Recipes with tracked ingredients, used to compute and apply stock consumption.</summary>
        Task<IReadOnlyList<ProductRecipe>> GetByProductIdsAsync(IEnumerable<Guid> productIds, CancellationToken ct = default);

        Task<ProductRecipe?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid productId, Guid ingredientId, CancellationToken ct = default);
        Task AddAsync(ProductRecipe recipe, CancellationToken ct = default);
        void Remove(ProductRecipe recipe);
        void RemoveRange(IEnumerable<ProductRecipe> recipes);
    }

    public interface ICustomerRepository
    {
        Task<PagedResult<Customer>> GetPagedAsync(PageQuery page, string? search, CancellationToken ct = default);
        Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<Customer?> GetByPhoneAsync(string phoneNumber, CancellationToken ct = default);
        Task<bool> PhoneExistsAsync(string phoneNumber, Guid? excludeId = null, CancellationToken ct = default);
        Task<bool> HasOrdersAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(Customer customer, CancellationToken ct = default);
        void Remove(Customer customer);
    }

    public interface IOrderRepository
    {
        Task<PagedResult<Order>> GetPagedAsync(PageQuery page, Guid? customerId, OrderStatus? status, CancellationToken ct = default);
        Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(Order order, CancellationToken ct = default);
        Task<decimal> GetPaidSalesAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
        Task<int> CountCompletedAsync(CancellationToken ct = default);
    }

    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default);
        Task<IReadOnlyList<RefreshToken>> GetActiveByUserAsync(Guid userId, CancellationToken ct = default);
        Task AddAsync(RefreshToken token, CancellationToken ct = default);
    }
}
