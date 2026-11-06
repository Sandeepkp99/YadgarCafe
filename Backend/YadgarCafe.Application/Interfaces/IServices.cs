using YadgarCafe.Application.Common;
using YadgarCafe.Application.DTOs;
using YadgarCafe.Application.DTOs.Auth;
using YadgarCafe.Domain.Enums;

namespace YadgarCafe.Application.Interfaces
{
    // Services signal failure by throwing the exceptions in YadgarCafe.Common.Exceptions
    // (NotFound, Conflict, BadRequest, ...); the API maps them to HTTP responses centrally.

    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
        Task<UserDto> CreateStaffUserAsync(CreateStaffUserRequest request, CancellationToken ct = default);
        Task<AuthTokenResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
        Task<AuthTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
        Task LogoutAsync(Guid userId, CancellationToken ct = default);
        Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);
    }

    public interface ICategoryService
    {
        Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken ct = default);
        Task<CategoryResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<CategoryResponse> CreateAsync(CategoryRequest request, CancellationToken ct = default);
        Task<CategoryResponse> UpdateAsync(Guid id, CategoryRequest request, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }

    public interface IProductService
    {
        Task<PagedResult<ProductResponse>> GetAllAsync(PageQuery page, Guid? categoryId, string? search, CancellationToken ct = default);
        Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<ProductResponse> CreateAsync(ProductRequest request, CancellationToken ct = default);
        Task<ProductResponse> UpdateAsync(Guid id, ProductRequest request, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }

    public interface IIngredientService
    {
        Task<PagedResult<IngredientResponse>> GetAllAsync(PageQuery page, bool lowStockOnly, CancellationToken ct = default);
        Task<IngredientResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IngredientResponse> CreateAsync(IngredientRequest request, CancellationToken ct = default);
        Task<IngredientResponse> UpdateAsync(Guid id, IngredientRequest request, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }

    public interface IInventoryService
    {
        Task<PagedResult<InventoryResponse>> GetLedgerAsync(PageQuery page, Guid? ingredientId, int? days, CancellationToken ct = default);
        Task<InventoryResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<InventoryResponse> RecordMovementAsync(StockMovementRequest request, CancellationToken ct = default);
    }

    public interface IProductRecipeService
    {
        Task<IReadOnlyList<RecipeResponse>> GetAllAsync(CancellationToken ct = default);
        Task<ProductRecipeDetailResponse> GetByProductAsync(Guid productId, CancellationToken ct = default);
        Task<RecipeResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<RecipeResponse> CreateAsync(CreateRecipeRequest request, CancellationToken ct = default);
        Task<RecipeResponse> UpdateAsync(Guid id, UpdateRecipeRequest request, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task DeleteAllByProductAsync(Guid productId, CancellationToken ct = default);
    }

    public interface ICustomerService
    {
        Task<PagedResult<CustomerResponse>> GetAllAsync(PageQuery page, string? search, CancellationToken ct = default);
        Task<CustomerResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<CustomerResponse> GetByPhoneAsync(string phoneNumber, CancellationToken ct = default);
        Task<CustomerResponse> GetMyProfileAsync(CancellationToken ct = default);
        Task<CustomerResponse> CreateAsync(CustomerRequest request, CancellationToken ct = default);
        Task<CustomerResponse> UpdateAsync(Guid id, CustomerRequest request, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }

    public interface IOrderService
    {
        Task<PagedResult<OrderResponse>> GetAllAsync(PageQuery page, Guid? customerId, OrderStatus? status, CancellationToken ct = default);
        Task<OrderResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken ct = default);
        Task<OrderResponse> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken ct = default);
        Task<OrderResponse> UpdatePaymentStatusAsync(Guid id, PaymentStatus status, CancellationToken ct = default);
        Task<OrderResponse> CancelAsync(Guid id, CancellationToken ct = default);
    }

    public interface IDashboardService
    {
        Task<DashboardStatisticsResponse> GetStatisticsAsync(CancellationToken ct = default);
    }
}
