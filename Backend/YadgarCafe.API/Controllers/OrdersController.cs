using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YadgarCafe.Application.DTOs.Order;
using YadgarCafe.Application.Services.Interfaces;
using YadgarCafe.Domain.Enums;

namespace YadgarCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Get all orders
        /// </summary>
        /// <returns>List of all orders</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAll()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving orders.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get orders by customer
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>List of customer orders</returns>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetByCustomer(Guid customerId)
        {
            try
            {
                if (customerId == Guid.Empty)
                    return BadRequest(new { message = "Invalid customer ID." });

                var orders = await _orderService.GetOrdersByCustomerAsync(customerId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving orders.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <returns>Order details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponse>> GetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { message = "Invalid order ID." });

                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                    return NotFound(new { message = "Order not found." });

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the order.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        /// <param name="request">Order creation details</param>
        /// <returns>Created order</returns>
        [HttpPost]
        public async Task<ActionResult<OrderResponse>> Create([FromBody] CreateOrderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var order = await _orderService.CreateOrderAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
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
                    new { message = "An error occurred while creating the order.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update order status
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="status">New order status</param>
        /// <returns>Updated order</returns>
        [HttpPut("{id}/status")]
        public async Task<ActionResult<OrderResponse>> UpdateStatus(Guid id, [FromBody] OrderStatus status)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { message = "Invalid order ID." });

                var order = await _orderService.UpdateOrderStatusAsync(id, status);
                if (order == null)
                    return NotFound(new { message = "Order not found." });

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the order status.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update payment status
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="status">New payment status</param>
        /// <returns>Updated order</returns>
        [HttpPut("{id}/payment-status")]
        public async Task<ActionResult<OrderResponse>> UpdatePaymentStatus(Guid id, [FromBody] PaymentStatus status)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { message = "Invalid order ID." });

                var order = await _orderService.UpdatePaymentStatusAsync(id, status);
                if (order == null)
                    return NotFound(new { message = "Order not found." });

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the payment status.", error = ex.Message });
            }
        }

        /// <summary>
        /// Cancel an order
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <returns>Cancellation result</returns>
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<OrderResponse>> Cancel(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new { message = "Invalid order ID." });

                var result = await _orderService.CancelOrderAsync(id);
                if (!result)
                    return NotFound(new { message = "Order not found." });

                var order = await _orderService.GetOrderByIdAsync(id);
                return Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while cancelling the order.", error = ex.Message });
            }
        }
    }
}
