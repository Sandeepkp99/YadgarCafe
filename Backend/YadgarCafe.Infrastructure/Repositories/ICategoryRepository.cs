using YadgarCafe.Domain.Entities;

namespace YadgarCafe.Infrastructure.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(Guid id);
        Task<Category> AddAsync(Category category);
        Task<Category?> UpdateAsync(Category category);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsByNameAsync(string name);
    }
}
