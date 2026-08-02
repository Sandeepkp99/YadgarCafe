using YadgarCafe.Application.DTOs.Product;

namespace YadgarCafe.Application.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponse>> GetAllProductsAsync();
        Task<IEnumerable<ProductResponse>> GetProductsByCategoryAsync(Guid categoryId);
        Task<ProductResponse?> GetProductByIdAsync(Guid id);
        Task<ProductResponse> CreateProductAsync(CreateProductRequest request);
        Task<ProductResponse?> UpdateProductAsync(Guid id, UpdateProductRequest request);
        Task<bool> DeleteProductAsync(Guid id);
    }
}
