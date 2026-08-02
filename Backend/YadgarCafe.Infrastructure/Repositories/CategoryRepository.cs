using Microsoft.EntityFrameworkCore;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Infrastructure.Persistence;

namespace YadgarCafe.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(Guid id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<Category> AddAsync(Category category)
        {
            category.CreatedOn = DateTime.UtcNow;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category?> UpdateAsync(Category category)
        {
            var existingCategory = await GetByIdAsync(category.Id);
            if (existingCategory == null)
                return null;

            existingCategory.Name = category.Name;
            existingCategory.ModifiedOn = DateTime.UtcNow;

            _context.Categories.Update(existingCategory);
            await _context.SaveChangesAsync();

            return existingCategory;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var category = await GetByIdAsync(id);
            if (category == null)
                return false;

            category.IsDeleted = true;
            category.ModifiedOn = DateTime.UtcNow;

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Categories
                .AnyAsync(c => c.Name == name && !c.IsDeleted);
        }
    }
}
