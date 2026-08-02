using YadgarCafe.Application.DTOs.Order;
using YadgarCafe.Domain.Enums;

namespace YadgarCafe.Application.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponse>> GetAllOrdersAsync();
        Task<IEnumerable<OrderResponse>> GetOrdersByCustomerAsync(Guid customerId);
        Task<OrderResponse?> GetOrderByIdAsync(Guid id);
        Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
        Task<OrderResponse?> UpdateOrderStatusAsync(Guid id, OrderStatus status);
        Task<OrderResponse?> UpdatePaymentStatusAsync(Guid id, PaymentStatus status);
        Task<bool> CancelOrderAsync(Guid id);
        Task<decimal> GetTodaysSalesAsync();
        Task<decimal> GetMonthlySalesAsync();
        Task<int> GetTotalOrdersAsync();
    }
}
