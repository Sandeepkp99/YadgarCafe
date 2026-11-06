using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YadgarCafe.Application.Common;
using YadgarCafe.Application.DTOs;
using YadgarCafe.Application.Interfaces;
using YadgarCafe.Common.Constants;

namespace YadgarCafe.API.Controllers
{
    // The public menu (categories, products) is readable without login; changes need Admin or Staff.

    [Authorize(Roles = Roles.AdminOrStaff)]
    public class CategoriesController : ApiControllerBase
    {
        private readonly ICategoryService _service;

        public CategoriesController(ICategoryService service) => _service = service;

        [HttpGet, AllowAnonymous]
        public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAll(CancellationToken ct) =>
            Ok(await _service.GetAllAsync(ct));

        [HttpGet("{id:guid}"), AllowAnonymous]
        public async Task<ActionResult<CategoryResponse>> GetById(Guid id, CancellationToken ct) =>
            Ok(await _service.GetByIdAsync(id, ct));

        [HttpPost]
        public async Task<ActionResult<CategoryResponse>> Create(CategoryRequest request, CancellationToken ct)
        {
            var created = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<CategoryResponse>> Update(Guid id, CategoryRequest request, CancellationToken ct) =>
            Ok(await _service.UpdateAsync(id, request, ct));

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
    }

    [Authorize(Roles = Roles.AdminOrStaff)]
    public class ProductsController : ApiControllerBase
    {
        private readonly IProductService _service;

        public ProductsController(IProductService service) => _service = service;

        [HttpGet, AllowAnonymous]
        public async Task<ActionResult<PagedResult<ProductResponse>>> GetAll(
            [FromQuery] PageQuery page, [FromQuery] Guid? categoryId, [FromQuery] string? search, CancellationToken ct) =>
            Ok(await _service.GetAllAsync(page, categoryId, search, ct));

        [HttpGet("{id:guid}"), AllowAnonymous]
        public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken ct) =>
            Ok(await _service.GetByIdAsync(id, ct));

        [HttpPost]
        public async Task<ActionResult<ProductResponse>> Create(ProductRequest request, CancellationToken ct)
        {
            var created = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ProductResponse>> Update(Guid id, ProductRequest request, CancellationToken ct) =>
            Ok(await _service.UpdateAsync(id, request, ct));

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
    }

    // Ingredients, stock and recipes are internal operations data: staff only.

    [Authorize(Roles = Roles.AdminOrStaff)]
    public class IngredientsController : ApiControllerBase
    {
        private readonly IIngredientService _service;

        public IngredientsController(IIngredientService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<PagedResult<IngredientResponse>>> GetAll(
            [FromQuery] PageQuery page, [FromQuery] bool lowStockOnly, CancellationToken ct) =>
            Ok(await _service.GetAllAsync(page, lowStockOnly, ct));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<IngredientResponse>> GetById(Guid id, CancellationToken ct) =>
            Ok(await _service.GetByIdAsync(id, ct));

        [HttpPost]
        public async Task<ActionResult<IngredientResponse>> Create(IngredientRequest request, CancellationToken ct)
        {
            var created = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <remarks>Stock is changed through POST /api/inventories, never here.</remarks>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<IngredientResponse>> Update(Guid id, IngredientRequest request, CancellationToken ct) =>
            Ok(await _service.UpdateAsync(id, request, ct));

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
    }

    [Authorize(Roles = Roles.AdminOrStaff)]
    public class InventoriesController : ApiControllerBase
    {
        private readonly IInventoryService _service;

        public InventoriesController(IInventoryService service) => _service = service;

        /// <summary>Stock ledger. Filter by ingredient and/or the last N days.</summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<InventoryResponse>>> GetLedger(
            [FromQuery] PageQuery page, [FromQuery] Guid? ingredientId, [FromQuery] int? days, CancellationToken ct) =>
            Ok(await _service.GetLedgerAsync(page, ingredientId, days, ct));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<InventoryResponse>> GetById(Guid id, CancellationToken ct) =>
            Ok(await _service.GetByIdAsync(id, ct));

        /// <summary>Record a purchase, opening balance, adjustment or wastage. Updates the ingredient's stock.</summary>
        [HttpPost]
        public async Task<ActionResult<InventoryResponse>> RecordMovement(StockMovementRequest request, CancellationToken ct)
        {
            var created = await _service.RecordMovementAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
    }

    [Authorize(Roles = Roles.AdminOrStaff)]
    public class ProductRecipesController : ApiControllerBase
    {
        private readonly IProductRecipeService _service;

        public ProductRecipesController(IProductRecipeService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<RecipeResponse>>> GetAll(CancellationToken ct) =>
            Ok(await _service.GetAllAsync(ct));

        /// <summary>Recipe of a product, including whether current stock can produce it.</summary>
        [HttpGet("product/{productId:guid}")]
        public async Task<ActionResult<ProductRecipeDetailResponse>> GetByProduct(Guid productId, CancellationToken ct) =>
            Ok(await _service.GetByProductAsync(productId, ct));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<RecipeResponse>> GetById(Guid id, CancellationToken ct) =>
            Ok(await _service.GetByIdAsync(id, ct));

        [HttpPost]
        public async Task<ActionResult<RecipeResponse>> Create(CreateRecipeRequest request, CancellationToken ct)
        {
            var created = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<RecipeResponse>> Update(Guid id, UpdateRecipeRequest request, CancellationToken ct) =>
            Ok(await _service.UpdateAsync(id, request, ct));

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }

        [HttpDelete("product/{productId:guid}")]
        public async Task<IActionResult> DeleteAllByProduct(Guid productId, CancellationToken ct)
        {
            await _service.DeleteAllByProductAsync(productId, ct);
            return NoContent();
        }
    }
}
