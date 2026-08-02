using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YadgarCafe.Application.DTOs.Inventory;
using YadgarCafe.Application.Services.Interfaces;

namespace YadgarCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventoriesController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoriesController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        /// <summary>
        /// Get all inventory records
        /// </summary>
        /// <returns>List of all inventory records</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<InventoryResponse>>> GetAll()
        {
            try
            {
                var inventories = await _inventoryService.GetAllInventoryAsync();
                return Ok(inventories);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving inventory records.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get inventory records by ingredient
        /// </summary>
        /// <param name="ingredientId">Ingredient ID</param>
        /// <returns>List of inventory records for the ingredient</returns>
        [HttpGet("ingredient/{ingredientId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<InventoryResponse>>> GetByIngredient(Guid ingredientId)
        {
            try
            {
                if (ingredientId == Guid.Empty)
                    return BadRequest(new { message = "Invalid ingredient ID." });

                var inventories = await _inventoryService.GetInventoryByIngredientAsync(ingredientId);
                return Ok(inventories);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving inventory records.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get low stock inventory records
        /// </summary>
        /// <returns>List of inventory records with low stock</returns>
        [HttpGet("low-stock")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<InventoryResponse>>> GetLowStock()
        {
            try
            {
                var inventories = await _inventoryService.GetLowStockInventoryAsync();
                return Ok(inventories);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving low stock inventory records.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get inventory history for an ingredient
        /// </summary>
        /// <param name="ingredientId">Ingredient ID</param>
        /// <param name="days">Number of days to look back (default: 30)</param>
        /// <returns>Inventory history</returns>
        [HttpGet("history/{ingredientId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<InventoryHistoryResponse>>> GetHistory(Guid ingredientId, [FromQuery] int days = 30)
        {
            try
            {
                if (ingredientId == Guid.Empty)
                    return BadRequest(new { message = "Invalid ingredient ID." });

                if (days <= 0)
                    return BadRequest(new { message = "Days must be greater than 0." });

                var history = await _inventoryService.GetInventoryHistoryAsync(ingredientId, days);
                return Ok(history);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving inventory history.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get inventory record by ID
        /// </summary>
        /// <param name="id">Inventory ID</param>
        /// <returns>Inventory record details</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<InventoryResponse>> GetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { message = "Invalid inventory ID." });

                var inventory = await _inventoryService.GetInventoryByIdAsync(id);
                if (inventory == null)
                    return NotFound(new { message = "Inventory record not found." });

                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the inventory record.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new inventory record
        /// </summary>
        /// <param name="request">Inventory creation details</param>
        /// <returns>Created inventory record</returns>
        [HttpPost]
        public async Task<ActionResult<InventoryResponse>> Create([FromBody] CreateInventoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var inventory = await _inventoryService.CreateInventoryAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = inventory.Id }, inventory);
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
                    new { message = "An error occurred while creating the inventory record.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing inventory record
        /// </summary>
        /// <param name="id">Inventory ID</param>
        /// <param name="request">Updated inventory details</param>
        /// <returns>Updated inventory record</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<InventoryResponse>> Update(Guid id, [FromBody] UpdateInventoryRequest request)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { message = "Invalid inventory ID." });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var inventory = await _inventoryService.UpdateInventoryAsync(id, request);
                if (inventory == null)
                    return NotFound(new { message = "Inventory record not found." });

                return Ok(inventory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the inventory record.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete an inventory record
        /// </summary>
        /// <param name="id">Inventory ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { message = "Invalid inventory ID." });

                var result = await _inventoryService.DeleteInventoryAsync(id);
                if (!result)
                    return NotFound(new { message = "Inventory record not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deleting the inventory record.", error = ex.Message });
            }
        }
    }
}
