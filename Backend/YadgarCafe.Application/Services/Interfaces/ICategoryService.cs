using YadgarCafe.Application.DTOs.Category;

namespace YadgarCafe.Application.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync();
        Task<CategoryResponse?> GetCategoryByIdAsync(Guid id);
        Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request);
        Task<CategoryResponse?> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
        Task<bool> DeleteCategoryAsync(Guid id);
    }
}
