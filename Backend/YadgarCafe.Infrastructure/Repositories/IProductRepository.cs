using YadgarCafe.Domain.Entities;

namespace YadgarCafe.Infrastructure.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId);
        Task<Product?> GetByIdAsync(Guid id);
        Task<Product> AddAsync(Product product);
        Task<Product?> UpdateAsync(Product product);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> CategoryExistsAsync(Guid categoryId);
    }
}
