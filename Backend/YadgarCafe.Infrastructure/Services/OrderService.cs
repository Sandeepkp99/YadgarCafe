using YadgarCafe.Application.DTOs.Order;
using YadgarCafe.Application.Services.Interfaces;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Domain.Enums;
using YadgarCafe.Infrastructure.Repositories;

namespace YadgarCafe.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IInventoryRepository _inventoryRepository;

        public OrderService(
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IProductService productService,
            ICustomerService customerService,
            IInventoryRepository inventoryRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productService = productService;
            _customerService = customerService;
            _inventoryRepository = inventoryRepository;
        }

        public async Task<IEnumerable<OrderResponse>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return await MapToResponsesAsync(orders);
        }

        public async Task<IEnumerable<OrderResponse>> GetOrdersByCustomerAsync(Guid customerId)
        {
            var orders = await _orderRepository.GetByCustomerAsync(customerId);
            return await MapToResponsesAsync(orders);
        }

        public async Task<OrderResponse?> GetOrderByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order != null ? await MapToResponseAsync(order) : null;
        }

        public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
        {
            if (request.CustomerId == Guid.Empty)
                throw new ArgumentException("Customer ID is required.");

            if (!request.Items.Any())
                throw new ArgumentException("Order must contain at least one item.");

            if (!await _orderRepository.CustomerExistsAsync(request.CustomerId))
                throw new InvalidOperationException("Customer not found.");

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero.");

                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Product with ID '{item.ProductId}' not found.");

                if (!product.IsAvailable)
                    throw new InvalidOperationException($"Product '{product.Name}' is not available.");

                // Check if ingredients are available for this product
                var canProduce = true; // This should check ProductRecipe availability
                if (!canProduce)
                    throw new InvalidOperationException($"Insufficient ingredients to produce '{product.Name}'.");

                var itemTotal = product.Price * item.Quantity;
                totalAmount += itemTotal;

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = itemTotal
                });
            }

            var order = new Order
            {
                CustomerId = request.CustomerId,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                CreatedBy = "System",
                CreatedOn = DateTime.UtcNow
            };

            var created = await _orderRepository.AddAsync(order);

            // Add order items
            foreach (var item in orderItems)
            {
                item.OrderId = created.Id;
            }
            await _orderItemRepository.AddRangeAsync(orderItems);

            // Deduct inventory
            await DeductInventoryAsync(orderItems);

            return await MapToResponseAsync(created);
        }

        public async Task<OrderResponse?> UpdateOrderStatusAsync(Guid id, OrderStatus status)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return null;

            order.Status = status;
            var updated = await _orderRepository.UpdateAsync(order);
            return updated != null ? await MapToResponseAsync(updated) : null;
        }

        public async Task<OrderResponse?> UpdatePaymentStatusAsync(Guid id, PaymentStatus status)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return null;

            order.PaymentStatus = status;
            var updated = await _orderRepository.UpdateAsync(order);
            return updated != null ? await MapToResponseAsync(updated) : null;
        }

        public async Task<bool> CancelOrderAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return false;

            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Cannot cancel a completed or already cancelled order.");

            order.Status = OrderStatus.Cancelled;
            await _orderRepository.UpdateAsync(order);

            // Restore inventory
            var items = await _orderItemRepository.GetByOrderAsync(id);
            await RestoreInventoryAsync(items);

            return true;
        }

        public async Task<decimal> GetTodaysSalesAsync()
        {
            return await _orderRepository.GetTodaysSalesAsync();
        }

        public async Task<decimal> GetMonthlySalesAsync()
        {
            return await _orderRepository.GetMonthlySalesAsync();
        }

        public async Task<int> GetTotalOrdersAsync()
        {
            return await _orderRepository.GetTotalOrdersAsync();
        }

        private async Task DeductInventoryAsync(List<OrderItem> items)
        {
            foreach (var item in items)
            {
                // In a real scenario, we would deduct from inventory based on the recipe
                // For now, we'll update the ingredient stock from the inventory
                await Task.Delay(0); // Placeholder
            }
        }

        private async Task RestoreInventoryAsync(IEnumerable<OrderItem> items)
        {
            foreach (var item in items)
            {
                // Restore inventory when order is cancelled
                await Task.Delay(0); // Placeholder
            }
        }

        private async Task<OrderResponse> MapToResponseAsync(Order order)
        {
            var customer = order.CustomerId != Guid.Empty
                ? await _customerService.GetCustomerByIdAsync(order.CustomerId)
                : null;

            var items = await _orderItemRepository.GetByOrderAsync(order.Id);
            var itemResponses = items.Select(oi => new OrderItemResponse
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? "Unknown",
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice
            });

            return new OrderResponse
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Customer = customer,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                Items = itemResponses,
                CreatedOn = order.CreatedOn,
                ModifiedOn = order.ModifiedOn
            };
        }

        private async Task<IEnumerable<OrderResponse>> MapToResponsesAsync(IEnumerable<Order> orders)
        {
            var responses = new List<OrderResponse>();
            foreach (var order in orders)
            {
                responses.Add(await MapToResponseAsync(order));
            }
            return responses;
        }
    }
}
