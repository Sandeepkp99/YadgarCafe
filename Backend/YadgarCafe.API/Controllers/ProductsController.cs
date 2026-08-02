using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YadgarCafe.Application.DTOs.Product;
using YadgarCafe.Application.Services.Interfaces;

namespace YadgarCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        /// <returns>List of all products</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving products.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>List of products in the category</returns>
        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductResponse>>> GetByCategory(Guid categoryId)
        {
            try
            {
                if (categoryId == Guid.Empty)
                    return BadRequest(new { message = "Invalid category ID." });

                var products = await _productService.GetProductsByCategoryAsync(categoryId);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving products.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product details</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductResponse>> GetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { message = "Invalid product ID." });

                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                    return NotFound(new { message = "Product not found." });

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the product.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="request">Product creation details</param>
        /// <returns>Created product</returns>
        [HttpPost]
        public async Task<ActionResult<ProductResponse>> Create([FromBody] CreateProductRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var product = await _productService.CreateProductAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
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
                    new { message = "An error occurred while creating the product.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="request">Updated product details</param>
        /// <returns>Updated product</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductResponse>> Update(Guid id, [FromBody] UpdateProductRequest request)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { message = "Invalid product ID." });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var product = await _productService.UpdateProductAsync(id, request);
                if (product == null)
                    return NotFound(new { message = "Product not found." });

                return Ok(product);
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
                    new { message = "An error occurred while updating the product.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { message = "Invalid product ID." });

                var result = await _productService.DeleteProductAsync(id);
                if (!result)
                    return NotFound(new { message = "Product not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deleting the product.", error = ex.Message });
            }
        }
    }
}
