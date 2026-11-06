using YadgarCafe.Application.Common;
using YadgarCafe.Application.DTOs;
using YadgarCafe.Application.Interfaces;
using YadgarCafe.Application.Mappings;
using YadgarCafe.Common.Exceptions;
using YadgarCafe.Domain.Entities;

namespace YadgarCafe.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categories;
        private readonly IUnitOfWork _uow;

        public CategoryService(ICategoryRepository categories, IUnitOfWork uow)
        {
            _categories = categories;
            _uow = uow;
        }

        public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken ct = default) =>
            (await _categories.GetAllAsync(ct)).Select(c => c.ToResponse()).ToList();

        public async Task<CategoryResponse> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            (await Find(id, ct)).ToResponse();

        public async Task<CategoryResponse> CreateAsync(CategoryRequest request, CancellationToken ct = default)
        {
            var name = request.Name.Trim();
            if (await _categories.NameExistsAsync(name, ct: ct))
                throw new ConflictException($"A category named '{name}' already exists.");

            var category = new Category { Name = name };
            await _categories.AddAsync(category, ct);
            await _uow.SaveChangesAsync(ct);
            return category.ToResponse();
        }

        public async Task<CategoryResponse> UpdateAsync(Guid id, CategoryRequest request, CancellationToken ct = default)
        {
            var category = await Find(id, ct);
            var name = request.Name.Trim();

            if (await _categories.NameExistsAsync(name, id, ct))
                throw new ConflictException($"A category named '{name}' already exists.");

            category.Name = name;
            await _uow.SaveChangesAsync(ct);
            return category.ToResponse();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var category = await Find(id, ct);

            if (await _categories.HasProductsAsync(id, ct))
                throw new ConflictException("This category still has products. Move or delete them first.");

            _categories.Remove(category);
            await _uow.SaveChangesAsync(ct);
        }

        private async Task<Category> Find(Guid id, CancellationToken ct) =>
            await _categories.GetByIdAsync(id, ct) ?? throw new NotFoundException("Category", id);
    }

    public class ProductService : IProductService
    {
        private readonly IProductRepository _products;
        private readonly ICategoryRepository _categories;
        private readonly IProductRecipeRepository _recipes;
        private readonly IUnitOfWork _uow;

        public ProductService(
            IProductRepository products,
            ICategoryRepository categories,
            IProductRecipeRepository recipes,
            IUnitOfWork uow)
        {
            _products = products;
            _categories = categories;
            _recipes = recipes;
            _uow = uow;
        }

        public async Task<PagedResult<ProductResponse>> GetAllAsync(
            PageQuery page, Guid? categoryId, string? search, CancellationToken ct = default) =>
            (await _products.GetPagedAsync(page, categoryId, search, ct)).Map(p => p.ToResponse());

        public async Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            (await Find(id, ct)).ToResponse();

        public async Task<ProductResponse> CreateAsync(ProductRequest request, CancellationToken ct = default)
        {
            var name = request.Name.Trim();
            var category = await _categories.GetByIdAsync(request.CategoryId, ct)
                ?? throw new NotFoundException("Category", request.CategoryId);

            if (await _products.NameExistsAsync(name, ct: ct))
                throw new ConflictException($"A product named '{name}' already exists.");

            var product = new Product
            {
                Name = name,
                Price = request.Price,
                IsAvailable = request.IsAvailable,
                CategoryId = category.Id,
                Category = category
            };

            await _products.AddAsync(product, ct);
            await _uow.SaveChangesAsync(ct);
            return product.ToResponse();
        }

        public async Task<ProductResponse> UpdateAsync(Guid id, ProductRequest request, CancellationToken ct = default)
        {
            var product = await Find(id, ct);
            var name = request.Name.Trim();

            var category = await _categories.GetByIdAsync(request.CategoryId, ct)
                ?? throw new NotFoundException("Category", request.CategoryId);

            if (await _products.NameExistsAsync(name, id, ct))
                throw new ConflictException($"A product named '{name}' already exists.");

            product.Name = name;
            product.Price = request.Price;
            product.IsAvailable = request.IsAvailable;
            product.CategoryId = category.Id;
            product.Category = category;

            await _uow.SaveChangesAsync(ct);
            return product.ToResponse();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var product = await Find(id, ct);

            // A deleted product's recipe is meaningless, so it goes with it (past orders keep their own snapshot).
            _recipes.RemoveRange(await _recipes.GetByProductAsync(id, ct));
            _products.Remove(product);
            await _uow.SaveChangesAsync(ct);
        }

        private async Task<Product> Find(Guid id, CancellationToken ct) =>
            await _products.GetByIdAsync(id, ct) ?? throw new NotFoundException("Product", id);
    }

    public class IngredientService : IIngredientService
    {
        private readonly IIngredientRepository _ingredients;
        private readonly IUnitOfWork _uow;

        public IngredientService(IIngredientRepository ingredients, IUnitOfWork uow)
        {
            _ingredients = ingredients;
            _uow = uow;
        }

        public async Task<PagedResult<IngredientResponse>> GetAllAsync(
            PageQuery page, bool lowStockOnly, CancellationToken ct = default) =>
            (await _ingredients.GetPagedAsync(page, lowStockOnly, ct)).Map(i => i.ToResponse());

        public async Task<IngredientResponse> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            (await Find(id, ct)).ToResponse();

        public async Task<IngredientResponse> CreateAsync(IngredientRequest request, CancellationToken ct = default)
        {
            var name = request.Name.Trim();
            if (await _ingredients.NameExistsAsync(name, ct: ct))
                throw new ConflictException($"An ingredient named '{name}' already exists.");

            var ingredient = new Ingredient
            {
                Name = name,
                Unit = request.Unit.Trim(),
                MinimumStock = request.MinimumStock
            };

            await _ingredients.AddAsync(ingredient, ct);
            await _uow.SaveChangesAsync(ct);
            return ingredient.ToResponse();
        }

        public async Task<IngredientResponse> UpdateAsync(Guid id, IngredientRequest request, CancellationToken ct = default)
        {
            var ingredient = await Find(id, ct);
            var name = request.Name.Trim();

            if (await _ingredients.NameExistsAsync(name, id, ct))
                throw new ConflictException($"An ingredient named '{name}' already exists.");

            ingredient.Name = name;
            ingredient.Unit = request.Unit.Trim();
            ingredient.MinimumStock = request.MinimumStock;

            await _uow.SaveChangesAsync(ct);
            return ingredient.ToResponse();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var ingredient = await Find(id, ct);

            if (await _ingredients.IsUsedInRecipesAsync(id, ct))
                throw new ConflictException("This ingredient is used in product recipes. Remove it from them first.");

            _ingredients.Remove(ingredient);
            await _uow.SaveChangesAsync(ct);
        }

        private async Task<Ingredient> Find(Guid id, CancellationToken ct) =>
            await _ingredients.GetByIdAsync(id, ct) ?? throw new NotFoundException("Ingredient", id);
    }
}
