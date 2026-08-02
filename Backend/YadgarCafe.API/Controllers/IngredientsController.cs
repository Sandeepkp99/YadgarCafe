using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YadgarCafe.Application.DTOs.Ingredient;
using YadgarCafe.Application.Services.Interfaces;

namespace YadgarCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IngredientsController : ControllerBase
    {
        private readonly IIngredientService _ingredientService;

        public IngredientsController(IIngredientService ingredientService)
        {
            _ingredientService = ingredientService;
        }

        /// <summary>
        /// Get all ingredients
        /// </summary>
        /// <returns>List of all ingredients</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<IngredientResponse>>> GetAll()
        {
            try
            {
                var ingredients = await _ingredientService.GetAllIngredientsAsync();
                return Ok(ingredients);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving ingredients.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get low stock ingredients
        /// </summary>
        /// <returns>List of ingredients with low stock</returns>
        [HttpGet("low-stock")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<IngredientResponse>>> GetLowStock()
        {
            try
            {
                var ingredients = await _ingredientService.GetLowStockIngredientsAsync();
                return Ok(ingredients);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving low stock ingredients.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get ingredient by ID
        /// </summary>
        /// <param name="id">Ingredient ID</param>
        /// <returns>Ingredient details</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<IngredientResponse>> GetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { message = "Invalid ingredient ID." });

                var ingredient = await _ingredientService.GetIngredientByIdAsync(id);
                if (ingredient == null)
                    return NotFound(new { message = "Ingredient not found." });

                return Ok(ingredient);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the ingredient.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new ingredient
        /// </summary>
        /// <param name="request">Ingredient creation details</param>
        /// <returns>Created ingredient</returns>
        [HttpPost]
        public async Task<ActionResult<IngredientResponse>> Create([FromBody] CreateIngredientRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ingredient = await _ingredientService.CreateIngredientAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = ingredient.Id }, ingredient);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the ingredient.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing ingredient
        /// </summary>
        /// <param name="id">Ingredient ID</param>
        /// <param name="request">Updated ingredient details</param>
        /// <returns>Updated ingredient</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<IngredientResponse>> Update(Guid id, [FromBody] UpdateIngredientRequest request)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { message = "Invalid ingredient ID." });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ingredient = await _ingredientService.UpdateIngredientAsync(id, request);
                if (ingredient == null)
                    return NotFound(new { message = "Ingredient not found." });

                return Ok(ingredient);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the ingredient.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update ingredient stock
        /// </summary>
        /// <param name="id">Ingredient ID</param>
        /// <param name="quantity">New stock quantity</param>
        /// <returns>Updated ingredient</returns>
        [HttpPatch("{id}/stock")]
        public async Task<ActionResult<IngredientResponse>> UpdateStock(Guid id, [FromQuery] decimal quantity)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { message = "Invalid ingredient ID." });

                if (quantity < 0)
                    return BadRequest(new { message = "Quantity cannot be negative." });

                var ingredient = await _ingredientService.UpdateStockAsync(id, quantity);
                if (ingredient == null)
                    return NotFound(new { message = "Ingredient not found." });

                return Ok(ingredient);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the ingredient stock.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete an ingredient
        /// </summary>
        /// <param name="id">Ingredient ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { message = "Invalid ingredient ID." });

                var result = await _ingredientService.DeleteIngredientAsync(id);
                if (!result)
                    return NotFound(new { message = "Ingredient not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deleting the ingredient.", error = ex.Message });
            }
        }
    }
}
