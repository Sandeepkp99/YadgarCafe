using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YadgarCafe.Application.DTOs.ProductRecipe;
using YadgarCafe.Application.Services.Interfaces;

namespace YadgarCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductRecipesController : ControllerBase
    {
        private readonly IProductRecipeService _productRecipeService;

        public ProductRecipesController(IProductRecipeService productRecipeService)
        {
            _productRecipeService = productRecipeService;
        }

        /// <summary>
        /// Get all product recipes
        /// </summary>
        /// <returns>List of all product recipes</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductRecipeResponse>>> GetAll()
        {
            try
            {
                var recipes = await _productRecipeService.GetAllRecipesAsync();
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving product recipes.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get recipes by product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>List of recipes for the product</returns>
        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductRecipeResponse>>> GetByProduct(Guid productId)
        {
            try
            {
                if (productId == Guid.Empty)
                    return BadRequest(new { message = "Invalid product ID." });

                var recipes = await _productRecipeService.GetRecipeByProductAsync(productId);
                return Ok(recipes);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving product recipes.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get product with complete recipe details
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Product with recipe and ingredient information</returns>
        [HttpGet("product/{productId}/details")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductWithRecipeResponse>> GetProductWithRecipe(Guid productId)
        {
            try
            {
                if (productId == Guid.Empty)
                    return BadRequest(new { message = "Invalid product ID." });

                var product = await _productRecipeService.GetProductWithRecipeAsync(productId);
                if (product == null)
                    return NotFound(new { message = "Product not found." });

                return Ok(product);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving product recipe details.", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if product can be produced with current stock
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Availability status</returns>
        [HttpGet("product/{productId}/can-produce")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> CanProduceProduct(Guid productId)
        {
            try
            {
                if (productId == Guid.Empty)
                    return BadRequest(new { message = "Invalid product ID." });

                var canProduce = await _productRecipeService.CanProduceProductAsync(productId);
                return Ok(new { productId, canProduce });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while checking product availability.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get recipe by ID
        /// </summary>
        /// <param name="id">Recipe ID</param>
        /// <returns>Recipe details</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductRecipeResponse>> GetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { message = "Invalid recipe ID." });

                var recipe = await _productRecipeService.GetRecipeByIdAsync(id);
                if (recipe == null)
                    return NotFound(new { message = "Recipe not found." });

                return Ok(recipe);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the recipe.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new product recipe
        /// </summary>
        /// <param name="request">Recipe creation details</param>
        /// <returns>Created recipe</returns>
        [HttpPost]
        public async Task<ActionResult<ProductRecipeResponse>> Create([FromBody] CreateProductRecipeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var recipe = await _productRecipeService.CreateRecipeAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = recipe.Id }, recipe);
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
                    new { message = "An error occurred while creating the recipe.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing recipe
        /// </summary>
        /// <param name="id">Recipe ID</param>
        /// <param name="request">Updated recipe details</param>
        /// <returns>Updated recipe</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductRecipeResponse>> Update(Guid id, [FromBody] UpdateProductRecipeRequest request)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { message = "Invalid recipe ID." });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var recipe = await _productRecipeService.UpdateRecipeAsync(id, request);
                if (recipe == null)
                    return NotFound(new { message = "Recipe not found." });

                return Ok(recipe);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the recipe.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a recipe
        /// </summary>
        /// <param name="id">Recipe ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { message = "Invalid recipe ID." });

                var result = await _productRecipeService.DeleteRecipeAsync(id);
                if (!result)
                    return NotFound(new { message = "Recipe not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deleting the recipe.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete all recipes for a product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("product/{productId}")]
        public async Task<ActionResult> DeleteAllByProduct(Guid productId)
        {
            try
            {
                if (productId == Guid.Empty)
                    return BadRequest(new { message = "Invalid product ID." });

                var result = await _productRecipeService.DeleteAllRecipesByProductAsync(productId);
                if (!result)
                    return NotFound(new { message = "No recipes found for this product." });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deleting product recipes.", error = ex.Message });
            }
        }
    }
}
