using Microsoft.EntityFrameworkCore;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Infrastructure.Persistence;

namespace YadgarCafe.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<Product> AddAsync(Product product)
        {
            product.CreatedOn = DateTime.UtcNow;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> UpdateAsync(Product product)
        {
            var existingProduct = await GetByIdAsync(product.Id);
            if (existingProduct == null)
                return null;

            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.IsAvailable = product.IsAvailable;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.ModifiedOn = DateTime.UtcNow;

            _context.Products.Update(existingProduct);
            await _context.SaveChangesAsync();

            return existingProduct;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var product = await GetByIdAsync(id);
            if (product == null)
                return false;

            product.IsDeleted = true;
            product.ModifiedOn = DateTime.UtcNow;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Products
                .AnyAsync(p => p.Name == name && !p.IsDeleted);
        }

        public async Task<bool> CategoryExistsAsync(Guid categoryId)
        {
            return await _context.Categories
                .AnyAsync(c => c.Id == categoryId && !c.IsDeleted);
        }
    }
}
