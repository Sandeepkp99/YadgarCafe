using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YadgarCafe.Application.Services.Interfaces;

namespace YadgarCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IIngredientService _ingredientService;

        public DashboardController(
            IOrderService orderService,
            IProductService productService,
            IIngredientService ingredientService)
        {
            _orderService = orderService;
            _productService = productService;
            _ingredientService = ingredientService;
        }

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        /// <returns>Dashboard data with all statistics</returns>
        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetStatistics()
        {
            try
            {
                var todaysSales = await _orderService.GetTodaysSalesAsync();
                var monthlySales = await _orderService.GetMonthlySalesAsync();
                var totalOrders = await _orderService.GetTotalOrdersAsync();
                var lowStockItems = await _ingredientService.GetLowStockIngredientsAsync();

                return Ok(new
                {
                    todaysSales,
                    monthlySales,
                    totalOrders,
                    lowStockCount = lowStockItems.Count(),
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving statistics.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get today's sales
        /// </summary>
        /// <returns>Today's total sales amount</returns>
        [HttpGet("today-sales")]
        public async Task<ActionResult<decimal>> GetTodaysSales()
        {
            try
            {
                var sales = await _orderService.GetTodaysSalesAsync();
                return Ok(new { todaysSales = sales, date = DateTime.UtcNow.Date });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving today's sales.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get monthly sales
        /// </summary>
        /// <returns>Current month's total sales amount</returns>
        [HttpGet("monthly-sales")]
        public async Task<ActionResult<decimal>> GetMonthlySales()
        {
            try
            {
                var sales = await _orderService.GetMonthlySalesAsync();
                var now = DateTime.UtcNow;
                return Ok(new 
                { 
                    monthlySales = sales, 
                    month = $"{now.Year}-{now.Month:D2}" 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving monthly sales.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get total orders count
        /// </summary>
        /// <returns>Total number of completed orders</returns>
        [HttpGet("total-orders")]
        public async Task<ActionResult<int>> GetTotalOrders()
        {
            try
            {
                var total = await _orderService.GetTotalOrdersAsync();
                return Ok(new { totalOrders = total });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving total orders.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get low stock items
        /// </summary>
        /// <returns>List of ingredients with low stock</returns>
        [HttpGet("low-stock")]
        public async Task<ActionResult<object>> GetLowStock()
        {
            try
            {
                var items = await _ingredientService.GetLowStockIngredientsAsync();
                return Ok(new 
                { 
                    lowStockItems = items,
                    count = items.Count()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving low stock items.", error = ex.Message });
            }
        }
    }
}
