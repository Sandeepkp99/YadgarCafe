using YadgarCafe.Application.Common;
using YadgarCafe.Application.DTOs;
using YadgarCafe.Application.Interfaces;
using YadgarCafe.Application.Mappings;
using YadgarCafe.Common.Exceptions;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Domain.Enums;

namespace YadgarCafe.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventory;
        private readonly IIngredientRepository _ingredients;
        private readonly IUnitOfWork _uow;

        public InventoryService(IInventoryRepository inventory, IIngredientRepository ingredients, IUnitOfWork uow)
        {
            _inventory = inventory;
            _ingredients = ingredients;
            _uow = uow;
        }

        public async Task<PagedResult<InventoryResponse>> GetLedgerAsync(
            PageQuery page, Guid? ingredientId, int? days, CancellationToken ct = default)
        {
            if (days is <= 0)
                throw new BadRequestException("Days must be greater than 0.");

            DateTime? from = days is null ? null : DateTime.UtcNow.AddDays(-days.Value);
            return (await _inventory.GetPagedAsync(page, ingredientId, from, ct)).Map(i => i.ToResponse());
        }

        public async Task<InventoryResponse> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            (await _inventory.GetByIdAsync(id, ct) ?? throw new NotFoundException("Inventory entry", id)).ToResponse();

        public async Task<InventoryResponse> RecordMovementAsync(StockMovementRequest request, CancellationToken ct = default)
        {
            var ingredient = await _ingredients.GetByIdAsync(request.IngredientId, ct)
                ?? throw new NotFoundException("Ingredient", request.IngredientId);

            var increases = request.Type switch
            {
                StockMovementType.OpeningBalance or StockMovementType.Purchase => true,
                StockMovementType.Wastage => false,
                StockMovementType.Adjustment => request.IsIncrease,
                _ => throw new BadRequestException("This movement type cannot be recorded manually.")
            };

            var entry = ingredient.ApplyMovement(
                request.Type, increases ? request.Quantity : -request.Quantity, request.Remarks);
            entry.Ingredient = ingredient;

            await _inventory.AddAsync(entry, ct);
            await _uow.SaveChangesAsync(ct);
            return entry.ToResponse();
        }
    }

    public class ProductRecipeService : IProductRecipeService
    {
        private readonly IProductRecipeRepository _recipes;
        private readonly IProductRepository _products;
        private readonly IIngredientRepository _ingredients;
        private readonly IUnitOfWork _uow;

        public ProductRecipeService(
            IProductRecipeRepository recipes,
            IProductRepository products,
            IIngredientRepository ingredients,
            IUnitOfWork uow)
        {
            _recipes = recipes;
            _products = products;
            _ingredients = ingredients;
            _uow = uow;
        }

        public async Task<IReadOnlyList<RecipeResponse>> GetAllAsync(CancellationToken ct = default) =>
            (await _recipes.GetAllAsync(ct)).Select(r => r.ToResponse()).ToList();

        public async Task<ProductRecipeDetailResponse> GetByProductAsync(Guid productId, CancellationToken ct = default)
        {
            var product = await _products.GetByIdAsync(productId, ct)
                ?? throw new NotFoundException("Product", productId);

            var recipes = (await _recipes.GetByProductAsync(productId, ct)).Select(r => r.ToResponse()).ToList();
            var canProduce = recipes.Count > 0 && recipes.All(r => r.IsAvailable);

            return new ProductRecipeDetailResponse(product.Id, product.Name, product.Price, canProduce, recipes);
        }

        public async Task<RecipeResponse> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            (await Find(id, ct)).ToResponse();

        public async Task<RecipeResponse> CreateAsync(CreateRecipeRequest request, CancellationToken ct = default)
        {
            var product = await _products.GetByIdAsync(request.ProductId, ct)
                ?? throw new NotFoundException("Product", request.ProductId);
            var ingredient = await _ingredients.GetByIdAsync(request.IngredientId, ct)
                ?? throw new NotFoundException("Ingredient", request.IngredientId);

            if (await _recipes.ExistsAsync(product.Id, ingredient.Id, ct))
                throw new ConflictException("This ingredient is already part of the product's recipe.");

            var recipe = new ProductRecipe
            {
                ProductId = product.Id,
                Product = product,
                IngredientId = ingredient.Id,
                Ingredient = ingredient,
                Quantity = request.Quantity
            };

            await _recipes.AddAsync(recipe, ct);
            await _uow.SaveChangesAsync(ct);
            return recipe.ToResponse();
        }

        public async Task<RecipeResponse> UpdateAsync(Guid id, UpdateRecipeRequest request, CancellationToken ct = default)
        {
            var recipe = await Find(id, ct);
            recipe.Quantity = request.Quantity;
            await _uow.SaveChangesAsync(ct);
            return recipe.ToResponse();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            _recipes.Remove(await Find(id, ct));
            await _uow.SaveChangesAsync(ct);
        }

        public async Task DeleteAllByProductAsync(Guid productId, CancellationToken ct = default)
        {
            var recipes = await _recipes.GetByProductAsync(productId, ct);
            if (recipes.Count == 0)
                throw new NotFoundException($"Product with ID '{productId}' has no recipe.");

            _recipes.RemoveRange(recipes);
            await _uow.SaveChangesAsync(ct);
        }

        private async Task<ProductRecipe> Find(Guid id, CancellationToken ct) =>
            await _recipes.GetByIdAsync(id, ct) ?? throw new NotFoundException("Recipe", id);
    }
}
