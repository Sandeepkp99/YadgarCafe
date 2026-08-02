using YadgarCafe.Application.DTOs.Category;
using YadgarCafe.Application.Services.Interfaces;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Infrastructure.Repositories;

namespace YadgarCafe.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync()
        {
            var categories = await _repository.GetAllAsync();
            return categories.Select(MapToResponse);
        }

        public async Task<CategoryResponse?> GetCategoryByIdAsync(Guid id)
        {
            var category = await _repository.GetByIdAsync(id);
            return category != null ? MapToResponse(category) : null;
        }

        public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Category name is required.");

            if (await _repository.ExistsByNameAsync(request.Name))
                throw new InvalidOperationException($"A category with the name '{request.Name}' already exists.");

            var category = new Category
            {
                Name = request.Name,
                CreatedBy = "System",
                CreatedOn = DateTime.UtcNow
            };

            var created = await _repository.AddAsync(category);
            return MapToResponse(created);
        }

        public async Task<CategoryResponse?> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Category name is required.");

            var existingCategory = await _repository.GetByIdAsync(id);
            if (existingCategory == null)
                return null;

            if (existingCategory.Name != request.Name && await _repository.ExistsByNameAsync(request.Name))
                throw new InvalidOperationException($"A category with the name '{request.Name}' already exists.");

            var category = new Category
            {
                Id = id,
                Name = request.Name,
                ModifiedBy = "System"
            };

            var updated = await _repository.UpdateAsync(category);
            return updated != null ? MapToResponse(updated) : null;
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        private static CategoryResponse MapToResponse(Category category)
        {
            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                CreatedOn = category.CreatedOn,
                ModifiedOn = category.ModifiedOn
            };
        }
    }
}
