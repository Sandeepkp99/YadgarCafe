using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YadgarCafe.Application.Common;
using YadgarCafe.Application.DTOs;
using YadgarCafe.Application.Interfaces;
using YadgarCafe.Common.Constants;
using YadgarCafe.Domain.Enums;

namespace YadgarCafe.API.Controllers
{
    [Authorize]
    public class CustomersController : ApiControllerBase
    {
        private readonly ICustomerService _service;

        public CustomersController(ICustomerService service) => _service = service;

        [HttpGet]
        [Authorize(Roles = Roles.AdminOrStaff)]
        public async Task<ActionResult<PagedResult<CustomerResponse>>> GetAll(
            [FromQuery] PageQuery page, [FromQuery] string? search, CancellationToken ct) =>
            Ok(await _service.GetAllAsync(page, search, ct));

        /// <summary>The logged-in customer's own profile.</summary>
        [HttpGet("me")]
        public async Task<ActionResult<CustomerResponse>> GetMine(CancellationToken ct) =>
            Ok(await _service.GetMyProfileAsync(ct));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CustomerResponse>> GetById(Guid id, CancellationToken ct) =>
            Ok(await _service.GetByIdAsync(id, ct));

        [HttpGet("phone/{phoneNumber}")]
        [Authorize(Roles = Roles.AdminOrStaff)]
        public async Task<ActionResult<CustomerResponse>> GetByPhone(string phoneNumber, CancellationToken ct) =>
            Ok(await _service.GetByPhoneAsync(phoneNumber, ct));

        /// <summary>Create a walk-in customer.</summary>
        [HttpPost]
        [Authorize(Roles = Roles.AdminOrStaff)]
        public async Task<ActionResult<CustomerResponse>> Create(CustomerRequest request, CancellationToken ct)
        {
            var created = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<CustomerResponse>> Update(Guid id, CustomerRequest request, CancellationToken ct) =>
            Ok(await _service.UpdateAsync(id, request, ct));

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
    }

    [Authorize]
    public class OrdersController : ApiControllerBase
    {
        private readonly IOrderService _service;

        public OrdersController(IOrderService service) => _service = service;

        /// <summary>Staff see every order; customers only their own.</summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<OrderResponse>>> GetAll(
            [FromQuery] PageQuery page, [FromQuery] Guid? customerId, [FromQuery] OrderStatus? status, CancellationToken ct) =>
            Ok(await _service.GetAllAsync(page, customerId, status, ct));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OrderResponse>> GetById(Guid id, CancellationToken ct) =>
            Ok(await _service.GetByIdAsync(id, ct));

        /// <summary>Places an order and deducts the ingredient stock atomically.</summary>
        [HttpPost]
        public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request, CancellationToken ct)
        {
            var created = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}/status")]
        [Authorize(Roles = Roles.AdminOrStaff)]
        public async Task<ActionResult<OrderResponse>> UpdateStatus(Guid id, UpdateOrderStatusRequest request, CancellationToken ct) =>
            Ok(await _service.UpdateStatusAsync(id, request.Status, ct));

        [HttpPut("{id:guid}/payment-status")]
        [Authorize(Roles = Roles.AdminOrStaff)]
        public async Task<ActionResult<OrderResponse>> UpdatePaymentStatus(Guid id, UpdatePaymentStatusRequest request, CancellationToken ct) =>
            Ok(await _service.UpdatePaymentStatusAsync(id, request.Status, ct));

        /// <summary>Cancels the order and returns its ingredients to stock. Customers can cancel only pending orders.</summary>
        [HttpPost("{id:guid}/cancel")]
        public async Task<ActionResult<OrderResponse>> Cancel(Guid id, CancellationToken ct) =>
            Ok(await _service.CancelAsync(id, ct));
    }

    [Authorize(Roles = Roles.AdminOrStaff)]
    public class DashboardController : ApiControllerBase
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service) => _service = service;

        [HttpGet("statistics")]
        public async Task<ActionResult<DashboardStatisticsResponse>> GetStatistics(CancellationToken ct) =>
            Ok(await _service.GetStatisticsAsync(ct));
    }
}
