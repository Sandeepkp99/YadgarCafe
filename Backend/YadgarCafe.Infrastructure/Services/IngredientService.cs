using YadgarCafe.Application.DTOs.Ingredient;
using YadgarCafe.Application.Services.Interfaces;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Infrastructure.Repositories;

namespace YadgarCafe.Infrastructure.Services
{
    public class IngredientService : IIngredientService
    {
        private readonly IIngredientRepository _repository;

        public IngredientService(IIngredientRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<IngredientResponse>> GetAllIngredientsAsync()
        {
            var ingredients = await _repository.GetAllAsync();
            return ingredients.Select(MapToResponse);
        }

        public async Task<IEnumerable<IngredientResponse>> GetLowStockIngredientsAsync()
        {
            var ingredients = await _repository.GetLowStockAsync();
            return ingredients.Select(MapToResponse);
        }

        public async Task<IngredientResponse?> GetIngredientByIdAsync(Guid id)
        {
            var ingredient = await _repository.GetByIdAsync(id);
            return ingredient != null ? MapToResponse(ingredient) : null;
        }

        public async Task<IngredientResponse> CreateIngredientAsync(CreateIngredientRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Ingredient name is required.");

            if (string.IsNullOrWhiteSpace(request.Unit))
                throw new ArgumentException("Unit of measurement is required.");

            if (request.CurrentStock < 0)
                throw new ArgumentException("Current stock cannot be negative.");

            if (request.MinimumStock < 0)
                throw new ArgumentException("Minimum stock cannot be negative.");

            if (await _repository.ExistsByNameAsync(request.Name))
                throw new InvalidOperationException($"An ingredient with the name '{request.Name}' already exists.");

            var ingredient = new Ingredient
            {
                Name = request.Name,
                Unit = request.Unit,
                CurrentStock = request.CurrentStock,
                MinimumStock = request.MinimumStock,
                CreatedBy = "System",
                CreatedOn = DateTime.UtcNow
            };

            var created = await _repository.AddAsync(ingredient);
            return MapToResponse(created);
        }

        public async Task<IngredientResponse?> UpdateIngredientAsync(Guid id, UpdateIngredientRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Ingredient name is required.");

            if (string.IsNullOrWhiteSpace(request.Unit))
                throw new ArgumentException("Unit of measurement is required.");

            if (request.CurrentStock < 0)
                throw new ArgumentException("Current stock cannot be negative.");

            if (request.MinimumStock < 0)
                throw new ArgumentException("Minimum stock cannot be negative.");

            var existingIngredient = await _repository.GetByIdAsync(id);
            if (existingIngredient == null)
                return null;

            if (existingIngredient.Name != request.Name && await _repository.ExistsByNameAsync(request.Name))
                throw new InvalidOperationException($"An ingredient with the name '{request.Name}' already exists.");

            var ingredient = new Ingredient
            {
                Id = id,
                Name = request.Name,
                Unit = request.Unit,
                CurrentStock = request.CurrentStock,
                MinimumStock = request.MinimumStock,
                ModifiedBy = "System"
            };

            var updated = await _repository.UpdateAsync(ingredient);
            return updated != null ? MapToResponse(updated) : null;
        }

        public async Task<IngredientResponse?> UpdateStockAsync(Guid id, decimal quantity)
        {
            var ingredient = await _repository.GetByIdAsync(id);
            if (ingredient == null)
                return null;

            ingredient.CurrentStock = quantity;
            ingredient.ModifiedBy = "System";

            var updated = await _repository.UpdateAsync(ingredient);
            return updated != null ? MapToResponse(updated) : null;
        }

        public async Task<bool> DeleteIngredientAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        private static IngredientResponse MapToResponse(Ingredient ingredient)
        {
            return new IngredientResponse
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                Unit = ingredient.Unit,
                CurrentStock = ingredient.CurrentStock,
                MinimumStock = ingredient.MinimumStock,
                CreatedOn = ingredient.CreatedOn,
                ModifiedOn = ingredient.ModifiedOn
            };
        }
    }
}
