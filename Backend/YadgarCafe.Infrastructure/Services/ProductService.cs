using YadgarCafe.Application.DTOs.Category;
using YadgarCafe.Application.DTOs.Product;
using YadgarCafe.Application.Services.Interfaces;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Infrastructure.Repositories;

namespace YadgarCafe.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly ICategoryService _categoryService;

        public ProductService(IProductRepository repository, ICategoryService categoryService)
        {
            _repository = repository;
            _categoryService = categoryService;
        }

        public async Task<IEnumerable<ProductResponse>> GetAllProductsAsync()
        {
            var products = await _repository.GetAllAsync();
            return await MapToResponsesAsync(products);
        }

        public async Task<IEnumerable<ProductResponse>> GetProductsByCategoryAsync(Guid categoryId)
        {
            var products = await _repository.GetByCategoryAsync(categoryId);
            return await MapToResponsesAsync(products);
        }

        public async Task<ProductResponse?> GetProductByIdAsync(Guid id)
        {
            var product = await _repository.GetByIdAsync(id);
            return product != null ? await MapToResponseAsync(product) : null;
        }

        public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Product name is required.");

            if (request.Price < 0)
                throw new ArgumentException("Product price cannot be negative.");

            if (request.CategoryId == Guid.Empty)
                throw new ArgumentException("Category ID is required.");

            if (!await _repository.CategoryExistsAsync(request.CategoryId))
                throw new InvalidOperationException($"Category with ID '{request.CategoryId}' does not exist.");

            if (await _repository.ExistsByNameAsync(request.Name))
                throw new InvalidOperationException($"A product with the name '{request.Name}' already exists.");

            var product = new Product
            {
                Name = request.Name,
                Price = request.Price,
                IsAvailable = request.IsAvailable,
                CategoryId = request.CategoryId,
                CreatedBy = "System",
                CreatedOn = DateTime.UtcNow
            };

            var created = await _repository.AddAsync(product);
            return await MapToResponseAsync(created);
        }

        public async Task<ProductResponse?> UpdateProductAsync(Guid id, UpdateProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Product name is required.");

            if (request.Price < 0)
                throw new ArgumentException("Product price cannot be negative.");

            if (request.CategoryId == Guid.Empty)
                throw new ArgumentException("Category ID is required.");

            if (!await _repository.CategoryExistsAsync(request.CategoryId))
                throw new InvalidOperationException($"Category with ID '{request.CategoryId}' does not exist.");

            var existingProduct = await _repository.GetByIdAsync(id);
            if (existingProduct == null)
                return null;

            if (existingProduct.Name != request.Name && await _repository.ExistsByNameAsync(request.Name))
                throw new InvalidOperationException($"A product with the name '{request.Name}' already exists.");

            var product = new Product
            {
                Id = id,
                Name = request.Name,
                Price = request.Price,
                IsAvailable = request.IsAvailable,
                CategoryId = request.CategoryId,
                ModifiedBy = "System"
            };

            var updated = await _repository.UpdateAsync(product);
            return updated != null ? await MapToResponseAsync(updated) : null;
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        private async Task<ProductResponse> MapToResponseAsync(Product product)
        {
            var category = product.CategoryId != Guid.Empty 
                ? await _categoryService.GetCategoryByIdAsync(product.CategoryId)
                : null;

            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                IsAvailable = product.IsAvailable,
                CategoryId = product.CategoryId,
                Category = category,
                CreatedOn = product.CreatedOn,
                ModifiedOn = product.ModifiedOn
            };
        }

        private async Task<IEnumerable<ProductResponse>> MapToResponsesAsync(IEnumerable<Product> products)
        {
            var responses = new List<ProductResponse>();
            foreach (var product in products)
            {
                responses.Add(await MapToResponseAsync(product));
            }
            return responses;
        }
    }
}
