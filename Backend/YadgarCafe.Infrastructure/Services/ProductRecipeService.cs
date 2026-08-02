using YadgarCafe.Application.DTOs.ProductRecipe;
using YadgarCafe.Application.Services.Interfaces;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Infrastructure.Repositories;

namespace YadgarCafe.Infrastructure.Services
{
    public class ProductRecipeService : IProductRecipeService
    {
        private readonly IProductRecipeRepository _repository;
        private readonly IProductService _productService;
        private readonly IIngredientService _ingredientService;

        public ProductRecipeService(
            IProductRecipeRepository repository,
            IProductService productService,
            IIngredientService ingredientService)
        {
            _repository = repository;
            _productService = productService;
            _ingredientService = ingredientService;
        }

        public async Task<IEnumerable<ProductRecipeResponse>> GetAllRecipesAsync()
        {
            var recipes = await _repository.GetAllAsync();
            return await MapToResponsesAsync(recipes);
        }

        public async Task<IEnumerable<ProductRecipeResponse>> GetRecipeByProductAsync(Guid productId)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("Product ID is required.");

            var recipes = await _repository.GetByProductAsync(productId);
            return await MapToResponsesAsync(recipes);
        }

        public async Task<ProductRecipeResponse?> GetRecipeByIdAsync(Guid id)
        {
            var recipe = await _repository.GetByIdAsync(id);
            return recipe != null ? await MapToResponseAsync(recipe) : null;
        }

        public async Task<ProductWithRecipeResponse?> GetProductWithRecipeAsync(Guid productId)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("Product ID is required.");

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return null;

            var recipes = await _repository.GetByProductAsync(productId);
            var ingredients = new List<RecipeIngredientResponse>();

            foreach (var recipe in recipes)
            {
                var ingredient = await _ingredientService.GetIngredientByIdAsync(recipe.IngredientId);
                if (ingredient != null)
                {
                    ingredients.Add(new RecipeIngredientResponse
                    {
                        IngredientId = ingredient.Id,
                        IngredientName = ingredient.Name,
                        Unit = ingredient.Unit,
                        Quantity = recipe.Quantity,
                        CurrentStock = ingredient.CurrentStock
                    });
                }
            }

            return new ProductWithRecipeResponse
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductPrice = product.Price,
                Ingredients = ingredients
            };
        }

        public async Task<ProductRecipeResponse> CreateRecipeAsync(CreateProductRecipeRequest request)
        {
            if (request.ProductId == Guid.Empty)
                throw new ArgumentException("Product ID is required.");

            if (request.IngredientId == Guid.Empty)
                throw new ArgumentException("Ingredient ID is required.");

            if (request.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            if (!await _repository.ProductExistsAsync(request.ProductId))
                throw new InvalidOperationException($"Product with ID '{request.ProductId}' does not exist.");

            if (!await _repository.IngredientExistsAsync(request.IngredientId))
                throw new InvalidOperationException($"Ingredient with ID '{request.IngredientId}' does not exist.");

            if (await _repository.RecipeExistsAsync(request.ProductId, request.IngredientId))
                throw new InvalidOperationException($"This ingredient is already added to the product recipe.");

            var recipe = new ProductRecipe
            {
                ProductId = request.ProductId,
                IngredientId = request.IngredientId,
                Quantity = request.Quantity
            };

            var created = await _repository.AddAsync(recipe);
            return await MapToResponseAsync(created);
        }

        public async Task<ProductRecipeResponse?> UpdateRecipeAsync(Guid id, UpdateProductRecipeRequest request)
        {
            if (request.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            var existingRecipe = await _repository.GetByIdAsync(id);
            if (existingRecipe == null)
                return null;

            var recipe = new ProductRecipe
            {
                Id = id,
                ProductId = existingRecipe.ProductId,
                IngredientId = existingRecipe.IngredientId,
                Quantity = request.Quantity
            };

            var updated = await _repository.UpdateAsync(recipe);
            return updated != null ? await MapToResponseAsync(updated) : null;
        }

        public async Task<bool> DeleteRecipeAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<bool> DeleteAllRecipesByProductAsync(Guid productId)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("Product ID is required.");

            return await _repository.DeleteAllByProductAsync(productId);
        }

        public async Task<bool> CanProduceProductAsync(Guid productId)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("Product ID is required.");

            var recipes = await _repository.GetByProductAsync(productId);
            if (!recipes.Any())
                return false;

            foreach (var recipe in recipes)
            {
                if (recipe.Ingredient.CurrentStock < recipe.Quantity)
                    return false;
            }

            return true;
        }

        private async Task<ProductRecipeResponse> MapToResponseAsync(ProductRecipe recipe)
        {
            var product = recipe.ProductId != Guid.Empty
                ? await _productService.GetProductByIdAsync(recipe.ProductId)
                : null;

            var ingredient = recipe.IngredientId != Guid.Empty
                ? await _ingredientService.GetIngredientByIdAsync(recipe.IngredientId)
                : null;

            return new ProductRecipeResponse
            {
                Id = recipe.Id,
                ProductId = recipe.ProductId,
                Product = product,
                IngredientId = recipe.IngredientId,
                Ingredient = ingredient,
                Quantity = recipe.Quantity
            };
        }

        private async Task<IEnumerable<ProductRecipeResponse>> MapToResponsesAsync(IEnumerable<ProductRecipe> recipes)
        {
            var responses = new List<ProductRecipeResponse>();
            foreach (var recipe in recipes)
            {
                responses.Add(await MapToResponseAsync(recipe));
            }
            return responses;
        }
    }
}
